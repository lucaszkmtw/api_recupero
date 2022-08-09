using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.ServiceModel.Financials;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class PaymentDocumentItemServices : ApplicationService
    {
        private IAutoQueryDb _autoQuery;
        private TenantRepository _tenantRepository;

        public PaymentDocumentItemServices(
            IAutoQueryDb autoQuery,
            TenantRepository tenantRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
        }

        public object Put(PaymentDocumentMethod.Post request)
        {
            return Db.Update((Domain.Financials.PaymentDocumentMethod)request);
        }

        public object Post(PaymentDocumentMethod.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Financials.PaymentDocumentMethod)request, true);
            return request;
        }

        public QueryResponse<PaymentDocumentMethod.QueryResult> Get(PaymentDocumentMethod.Find request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        
        public QueryResponse<PaymentDocumentMethod.QueryResultByType> Get(PaymentDocumentMethod.FindByPaymentMethod2 request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());

            // switch (request.Module)
            //{
            //    case Api.BusinessDocumentModule.AccountsPayables:
            //        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden

            //        //Por cuenta y orden???
            //        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
            //        //var receiverIds = clients.Select(x => x.PersonId).ToList();

            //        var receiverIds = new List<int>();
            //        receiverIds.Add(Session.Tenant.PersonId);
            //        q.And(w => Sql.In(w.ReceiverId, receiverIds));
            //        break;

            //    case Api.BusinessDocumentModule.AccountsReceivables:
            //        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
            //        var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
            //        //var receiverIds = clients.Select(x => x.PersonId).ToList();
            //        //var issuerIds = new List<int>();
            //        var issuerIds = clients.Select(x => x.PersonId).ToList();
            //        issuerIds.Add(Session.Tenant.PersonId);
            //        q.And(w => Sql.In(w.IssuerId, issuerIds));
            //        break;
            //    default:
            //        //Por cuenta y orden???
            //        //var clients1 = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
            //        //var receiverIds1 = clients1.Select(x => x.PersonId).ToList();
            //        var receiverIds1 = new List<int>();
            //        receiverIds1.Add(Session.Tenant.PersonId);
            //        var issuerIds1 = new List<int>();
            //        issuerIds1.Add(Session.Tenant.PersonId);
            //        q.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
            //        break;
            //}

            return _autoQuery.Execute(request, q);
        }

        public QueryResponse<PaymentDocumentMethod.QueryResultByType> Get(PaymentDocumentMethod.FindByPaymentMethod request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }
            
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        public LookupResult Get(PaymentDocumentMethod.LookupCheckStatus request)
        {
            //[{ id: 0, name: 'Emitido' }, { id: 1, name: 'Para aprobar' }, { id: 2, name: 'Aprobado' }, { id: 3, name: 'Rechazado' }, { id: 4, name: 'Pagado' }, { id: 5, name: 'Para entregar' }, { id: 6, name: 'Entregado' }]
            var items = new List<LookupItem>
            {
                new LookupItem {Id = 0, Text = "Emitido"},
                new LookupItem {Id = 1, Text = "Entregado"},
                new LookupItem {Id = 2, Text = "Depositado"},
                new LookupItem {Id = 3, Text = "Acreditado"},
                new LookupItem {Id = 4, Text = "Cancelado"},
                new LookupItem {Id = 5, Text = "Rechazado"}
            };

            var result = new LookupResult
            {
                Data = items,
                Total = items.Count
            };
            return result;
        }

        public LookupResult Get(PaymentDocumentMethod.LookupCashTransaction request)
        {
            var items = new List<LookupItem>
            {
                new LookupItem {Id = 0, Text = "Emitido"},
                new LookupItem {Id = 4, Text = "Cancelado"}
            };

            var result = new LookupResult
            {
                Data = items,
                Total = items.Count
            };
            return result;
        }

        public LookupResult Get(PaymentDocumentMethod.LookupBankTransfer request)
        {
            var items = new List<LookupItem>
            {
                new LookupItem {Id = 0, Text = "Emitido"},
                new LookupItem {Id = 4, Text = "Cancelado"}
            };

            var result = new LookupResult
            {
                Data = items,
                Total = items.Count
            };
            return result;
        }

        public object Get(PaymentDocumentMethod.Get request)
        {
            var model = Db.SingleById<Domain.Financials.PaymentDocumentMethod>(request.Id);
            return model;
        }

        public object Get(PaymentDocumentMethod.GetCheck request)
        {
            var model = Db.SingleById<Domain.Financials.PaymentDocumentMethod>(request.Id);
            return model;
        }

        public async Task<QueryResponse<PaymentDocumentMethod.QueryResultChecksInCustody>> Get(PaymentDocumentMethod.ChecksInCustody request)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);

            if (request.OrderBy == null && request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams())
                .Join<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentDocument>((pdm, pd) => pdm.IssuerPaymentDocumentId == pd.Id, Db.JoinAlias("IssuerPaymentDocument"))
                .Where<Domain.Financials.PaymentMethod>(w => w.TypeId == 3) // Tipo cheque
                .And<Domain.Financials.PaymentDocumentMethod>(w => w.Status == request.View) // View = PaymentDocumentMethodStatus
                .UnsafeAnd("((IssuerPaymentDocument.TypeId = 1 AND IssuerPaymentDocument.ReceiverId = {0}) OR (IssuerPaymentDocument.TypeId = 2 AND IssuerPaymentDocument.IssuerId = {0}))", tenant.PersonId);
            return _autoQuery.Execute(request, q);
        }

        public async Task<object> Post(PaymentDocumentMethod.PostChecksincustodyDeposit request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    
                    var methods = await Db.SelectAsync(Db.From<Domain.Financials.PaymentDocumentMethod>().Where(c => Sql.In(c.Id, request.PaymentDocumentMethodIds)));
                    foreach(var method in methods)
                    {
                        method.DepositDate = request.DepositDate;
                        method.DepositNumber = request.DepositNumber;
                        method.Status = (byte)Domain.Financials.PaymentDocumentMethodStatus.Deposited;
                        await Db.UpdateAsync(method);

                        var paymentDocument = await Db.SingleByIdAsync<Domain.Financials.PaymentDocument>(method.IssuerPaymentDocumentId);
                        var paymentDocumentType = await Db.SingleByIdAsync<Domain.Financials.PaymentDocumentType>(paymentDocument.TypeId);
                        if (paymentDocument != null)
                        {
                            // Genero el movimiento en la cuenta bancaria si es un cheque
                            if (method.PaymentMethodId == 3)
                            {
                                var bankAccountEntry = new Domain.Financials.BankAccountEntry
                                {
                                    BankAccountId = request.BankAccountId,
                                    Amount = method.Amount,
                                    Description = paymentDocumentType.Name + " nro. " + paymentDocument.Number,
                                    CreateDate = DateTime.UtcNow,
                                    LinkedDocumentId = paymentDocument.Id,
                                    LinkedDocumentTypeId = (short)paymentDocument.TypeId,
                                    PostingDate = paymentDocument.DocumentDate
                                };
                                await Db.InsertAsync(bankAccountEntry);
                            }

                            // Genero el movimiento en la cuenta operativa
                            var account = await Db.SingleByIdAsync<Domain.BusinessPartners.BusinessPartnerAccount>(paymentDocument.AccountId);
                            if (account != null)
                            {
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
                                    Description = paymentDocumentType.Name + " nro. " + paymentDocument.Number,
                                    CreateDate = DateTime.UtcNow,
                                    LinkedDocumentId = paymentDocument.Id,
                                    LinkedDocumentTypeId = (short)paymentDocument.TypeId,
                                    PostingDate = DateTime.UtcNow
                                };
                                await Db.InsertAsync(accountEntry);
                            }
                        }
                    }

                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }
    }
}