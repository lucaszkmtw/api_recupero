using System;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.BusinessDocuments;
using CentralOperativa.ServiceInterface.System.Workflows;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class PaymentDocumentService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(PaymentDocumentService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly TenantRepository _tenantRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;
        private readonly PaymentDocumentRepository _paymentDocumentRepository;

        public PaymentDocumentService(
            IAutoQueryDb autoQuery,
            TenantRepository tenantRepository,
            WorkflowInstanceRepository workflowInstanceRepository,
            BusinessPartnerRepository businessPartnerRepository,
            PaymentDocumentRepository paymentDocumentRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _paymentDocumentRepository = paymentDocumentRepository;
        }

        public async Task<object> Get(Api.QueryPaymentDocumentsRequest request)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var p = Request.GetRequestParams();
            if (p.ContainsKey("number"))
            {
                p.Add("numberContains", p["number"]);
                p.Remove("number");
            }

            var q = _autoQuery.CreateQuery(request, p);
            q.Join<Domain.Financials.PaymentDocument, Domain.Financials.PaymentDocumentType>()
                .Join<Domain.Financials.PaymentDocument, Domain.System.Persons.Person>((pd, i) => pd.IssuerId == i.Id, Db.JoinAlias("issuer"))
                .Join<Domain.Financials.PaymentDocument, Domain.System.Persons.Person>((pd, r) => pd.ReceiverId == r.Id, Db.JoinAlias("receiver"))
            .Select(@"PaymentDocuments.*
                , PaymentDocumentTypes.ShortName AS TypeName
                , issuer.Name AS IssuerName
                , receiver.Name AS ReceiverName");

            if (p.ContainsKey("typeName"))
            {
                q.Where<BusinessDocumentType>(w => w.ShortName == p["typeName"]);
            }

            if (p.ContainsKey("issuerName"))
            {
                q.UnsafeWhere("issuer.Name LIKE {0}", Utils.SqlLike(p["issuerName"]));
            }

            if (p.ContainsKey("receiverName"))
            {
                q.UnsafeWhere("receiver.Name LIKE {0}", Utils.SqlLike(p["receiverName"]));
            }

            //Filtrar por el tenant
            q.And(w => w.IssuerId == tenant.PersonId);

            var model = _autoQuery.Execute(request, q);
            return model;
        }

        public async Task<Api.GetPaymentDocumentResponse> Get(Api.GetPaymentDocumentRequest request)
        {
            return await _paymentDocumentRepository.GetPaymentDocument(Db, request.Id, !request.Edit);
        }

        public async Task<Api.GetPaymentDocumentResponse> Put(Api.PostPaymentDocumentRequest request)
        {
            Api.GetPaymentDocumentResponse model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    model = await _paymentDocumentRepository.UpdatePaymentDocument(Db, request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }

        public async Task<Api.GetPaymentDocumentResponse> Post(Api.PostPaymentDocumentRequest request)
        {
            Api.GetPaymentDocumentResponse model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var tenant = Db.SingleById<Domain.System.Tenant>(Session.TenantId);
                    request.IssuerId = tenant.PersonId;
                    model = await _paymentDocumentRepository.CreatePaymentDocument(Db, Session, request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }

        /// <summary>
        /// Método para confirmar un documento de pago (OP/Recibo) cuando esta en estado Emitida 0 y va a pasar a 1 Entregada
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Api.GetPaymentDocumentResponse> Post(Api.PostConfirmPaymentDocumentRequest request)
        {
            Api.GetPaymentDocumentResponse model = null;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var paymentDocument = await _paymentDocumentRepository.GetPaymentDocument(Db, request.PaymentDocumentGuid, true);
                    if (paymentDocument != null)
                    {
                        if (paymentDocument.Status != (byte)BusinessDocumentStatus.PendingDelivery)
                        {
                            throw new ApplicationException("Invalid payment document status");
                        }

                        paymentDocument.Status = (byte)BusinessDocumentStatus.Delivered;
                        await Db.UpdateAsync((Domain.Financials.PaymentDocument)paymentDocument);

                        // Si el paymentdocument esta linkeado a facturas les invoco el paso de workflow de aprobación si es que tiene
                        // y ademas le cambio el estado a Pagada
                        var linkedBusinessDocumentIds = paymentDocument.Items.Where(w => w.LinkedDocumentTypeId == 6)
                                .Select(x => x.LinkedDocumentId)
                                .Distinct();
                        foreach (var linkedBusinessDocumentId in linkedBusinessDocumentIds)
                        {
                            var businessDocument = await Db.SingleByIdAsync<BusinessDocument>(linkedBusinessDocumentId);
                            if (businessDocument.Status == BusinessDocumentStatus.Approved)
                            {
                                if (businessDocument.ApprovalWorkflowInstanceId.HasValue)
                                {
                                    await _workflowInstanceRepository.ApproveWorkflowInstance(Db, Session, businessDocument.ApprovalWorkflowInstanceId.Value);
                                }
                                else
                                {
                                    businessDocument.Status = BusinessDocumentStatus.Paid;
                                    await Db.UpdateAsync(businessDocument);
                                }
                            }
                        }

                        //Si el paymentdocument esta linkeado a préstamos hacer que avance el workflow para que el prestamo quede como pagado
                        linkedBusinessDocumentIds = paymentDocument.Items.Where(w => w.LinkedDocumentTypeId == 4)
                                .Select(x => x.LinkedDocumentId)
                                .Distinct()
                                .ToList();
                        foreach (var linkedBusinessDocumentId in linkedBusinessDocumentIds)
                        {
                            var loan = await Db.SingleByIdAsync<Domain.Loans.Loan>(linkedBusinessDocumentId);
                            if (loan.Status == Domain.Loans.LoanStatus.PendingPayment) // Domain.Loans.LoanStatus.ToExecute
                            {
                                if (loan.AuthorizationWorkflowInstanceId.HasValue)
                                {
                                    await _workflowInstanceRepository.ApproveWorkflowInstance(Db, Session, loan.AuthorizationWorkflowInstanceId.Value);
                                }
                                else
                                {
                                    loan.Status = Domain.Loans.LoanStatus.Paid;
                                    await Db.UpdateAsync(loan);
                                }


                                // Para generar un movimiento en la cuenta del cliente de la OP, busco el Id del Person del Loan que sea de tipo Applicant y
                                // con eso busco despues el BusinessPartner asociado a ese person y ahi obtengo el BusinessPartnerAccount
                                var loanPersons = await Db.SelectAsync(Db.From<Domain.Loans.LoanPerson>().Where(w => w.LoanId == loan.Id));
                                var applicantId = loanPersons.First(x => x.Role == Domain.Loans.LoanPersonRole.Applicant).PersonId;

                                var businessPartner = (await Db.SelectAsync(Db.From<Domain.BusinessPartners.BusinessPartner>().Where(w => w.TypeId == 1 && w.PersonId == applicantId))).SingleOrDefault();
                                var account = (await _businessPartnerRepository.GetBusinessPartner(Db, businessPartner.Id, true)).Accounts.Items.OrderBy(o => o.Id).FirstOrDefault();
                                var linkedBusinessDocuments = paymentDocument.Items;
                                foreach (var linkedBusinessDocument in linkedBusinessDocuments)
                                {
                                    var accountEntry = new Domain.BusinessPartners.BusinessPartnerAccountEntry
                                    {
                                        AccountId = account.Id,
                                        Amount = linkedBusinessDocument.AmountToPay * -1,
                                        Description = linkedBusinessDocument.Description,
                                        CreateDate = DateTime.UtcNow,
                                        LinkedDocumentId = paymentDocument.Id,
                                        LinkedDocumentTypeId = 4,
                                        PostingDate = DateTime.UtcNow
                                    };
                                    await Db.InsertAsync(accountEntry);
                                }
                            }
                        }

                        //Si el documento no esta linkeado a un prestamo, genero un movimiento a la cuenta seleccionada en el formulario
                        if (linkedBusinessDocumentIds.Count() == 0)
                        {
                            var account = await Db.SingleByIdAsync<Domain.BusinessPartners.BusinessPartnerAccount>(paymentDocument.AccountId);
                            if (account != null)
                            {
                                var type = await Db.SingleByIdAsync<Domain.Financials.PaymentDocumentType>(paymentDocument.TypeId);

                                var methods = paymentDocument.Methods;
                                foreach (var method in methods)
                                {
                                    var paymentMethod = await Db.SingleByIdAsync<Domain.Financials.PaymentMethod>(method.PaymentMethodId);

                                    //Si el documento es recibo y el metodo es cheque no genero movimiento
                                    if (paymentDocument.TypeId == 2 && paymentMethod.TypeId == 3)
                                    {
                                        continue;
                                    }

                                    //Si el metodo de pago es transferencia anoto el movimiento en la cuenta bancaria.
                                    if (paymentMethod.TypeId == 2)
                                    {
                                        var bankAccountEntry = new Domain.Financials.BankAccountEntry
                                        {
                                            BankAccountId = method.BankAccountId.Value,
                                            Amount = method.Amount,
                                            Description = type.Name + " nro. " + paymentDocument.Number,
                                            CreateDate = DateTime.UtcNow,
                                            LinkedDocumentId = paymentDocument.Id,
                                            LinkedDocumentTypeId = (short)paymentDocument.TypeId,
                                            PostingDate = paymentDocument.DocumentDate
                                        };
                                        await Db.InsertAsync(bankAccountEntry);
                                    }

                                    //Si el metodo de pago es cheque.
                                    if (paymentMethod.TypeId == 2)
                                    {

                                    }

                                    decimal amount;
                                    if (paymentDocument.TypeId == 2)
                                    {
                                        amount = method.Amount;
                                    }
                                    else
                                    {
                                        amount = method.Amount * -1;
                                    }

                                    var accountEntry = new Domain.BusinessPartners.BusinessPartnerAccountEntry
                                    {
                                        AccountId = account.Id,
                                        Amount = amount,
                                        Description = type.Name + " nro. " + paymentDocument.Number,
                                        CreateDate = DateTime.UtcNow,
                                        LinkedDocumentId = paymentDocument.Id,
                                        LinkedDocumentTypeId = (short)paymentDocument.TypeId,
                                        PostingDate = paymentDocument.DocumentDate
                                    };
                                    await Db.InsertAsync(accountEntry);
                                }
                            }
                        }

                        // Actualizo el estado de los medios de pago relacionados
                        var paymentDocumentMethods = await Db.SelectAsync(Db.From<Domain.Financials.PaymentDocumentMethod>().Where(w => w.IssuerPaymentDocumentId == paymentDocument.Id));
                        foreach (var paymentDocumentMethod in paymentDocumentMethods)
                        {
                            if (paymentDocumentMethod.Status == (byte)Domain.Financials.PaymentDocumentMethodStatus.Emmited)
                            {
                                paymentDocumentMethod.Status = (byte)Domain.Financials.PaymentDocumentMethodStatus.Received;
                                Db.Update(paymentDocumentMethod);
                            }
                        }

                        model = await _paymentDocumentRepository.GetPaymentDocument(Db, paymentDocument.Id);
                    }

                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }
    }
}