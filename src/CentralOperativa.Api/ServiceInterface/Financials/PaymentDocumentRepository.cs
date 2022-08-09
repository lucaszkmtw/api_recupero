using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.BusinessDocuments;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
//using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    public class PaymentDocumentRepository : ApplicationService
    {
        private readonly PersonRepository _personRepository;
        //private readonly WorkflowInstanceRepository _workflowInstanceRepository;

        public PaymentDocumentRepository(PersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Api.GetPaymentDocumentResponse> GetPaymentDocument(IDbConnection db, Guid guid, bool? includePersons = false)
        {
            var document = (await db.SelectAsync(db.From<Domain.Financials.PaymentDocument>().Where(W => W.Guid == guid))).SingleOrDefault();
            if (document != null)
            {
                return await GetPaymentDocument(db, document.Id, includePersons);
            }

            return null;
        }

        public async Task<Api.GetPaymentDocumentResponse> GetPaymentDocument(IDbConnection db, int id, bool? includePersons = false)
        {
            var document = (await db.SingleByIdAsync<Domain.Financials.PaymentDocument>(id)).ConvertTo<Api.GetPaymentDocumentResponse>();

            // Items
            var items = await db.SelectAsync<Domain.Financials.PaymentDocumentItem>(w => w.PaymentDocumentId == document.Id);
            document.Items.AddRange(items);

            // Methods
            var methods = await db.SelectAsync<Domain.Financials.PaymentDocumentMethod>(w => w.IssuerPaymentDocumentId == document.Id);
            foreach (var item in methods)
            {
                var itemModel = item.ConvertTo<Api.PaymentDocumentMethodResponse>();
                var method = await db.SingleByIdAsync<Domain.Financials.PaymentMethod>(item.PaymentMethodId);
                itemModel.PaymentMethodName = method.Name;
                if (item.BankAccountId != null)
                {
                    var bankAccount = await db.SingleByIdAsync<Domain.Financials.BankAccount>(item.BankAccountId);
                    if (bankAccount != null)
                    {
                        itemModel.BankAccountName = bankAccount.Description + "-" + bankAccount.Code;
                    }
                }

                document.Methods.Add(itemModel);
            }

            if (includePersons.HasValue && includePersons.Value)
            {
                document.Issuer = await _personRepository.GetPerson(db, document.IssuerId);
                document.Receiver = await _personRepository.GetPerson(db, document.ReceiverId);
            }

            return document;
        }

        public async Task<Api.GetPaymentDocumentResponse> CreatePaymentDocument(IDbConnection db, Session session, Api.PostPaymentDocumentRequest request)
        {
            var typeId = request.TypeId; // 1; // PaymentDocumentTypeId

            //Number
            var segment = "0001";
            if (typeId == 2)
            {
                segment = "0002";
            }
            var query = $"SELECT MAX(CAST(SUBSTRING(Number,6,8) AS NUMERIC)) Number FROM PaymentDocuments WHERE TypeId = {typeId} AND IssuerId = {request.IssuerId} AND SUBSTRING(Number, 1, 4) = '{segment}'";
            var currentNumber = db.Scalar<int>(query);
            var number = $"{segment}-{(currentNumber + 1).ToString().PadLeft(8, '0')}";

            var existing = await db.SelectAsync(db.From<Domain.Financials.PaymentDocument>().Where(w => w.IssuerId == request.IssuerId && w.ReceiverId == request.ReceiverId && w.Number == number));
            if (existing.Count > 0)
            {
                throw new Exception("ERR_PaymentDocument_AlreadyExists");
            }

            request.CreateDate = DateTime.UtcNow;
            request.CreatedBy = session.UserId;
            request.Guid = Guid.NewGuid();
            request.Amount = request.Items.Sum(x => x.AmountToPay);
            //request.TypeId = typeId; //Viene por parametro
            request.Number = number;

            // Si tiene medios de pago asociados y el monto de la suma de los medios de pago da igual al total del monto a pagar paso la OP a estado Para entregar
            if (request.Methods.Any() && request.Methods.Sum(x => x.Amount) == request.Items.Sum(x => x.AmountToPay))
            {
                request.Status = (byte)BusinessDocumentStatus.PendingDelivery;
            }

            request.Id = (int) await db.InsertAsync((Domain.Financials.PaymentDocument)request, true);
            await SaveCollections(db, request);


            return await GetPaymentDocument(db, request.Id, true);
        }

        public async Task<Api.GetPaymentDocumentResponse> UpdatePaymentDocument(IDbConnection db, Api.PostPaymentDocumentRequest request)
        {
            request.Amount = decimal.Round(request.Items.Sum(x => x.AmountToPay), 2);

            // Si tiene medios de pago asociados y el monto de la suma de los medios de pago da igual al total del monto a pagar paso la OP a estado Para entregar
            if (request.Methods.Any())
            {
                var methodsSum = decimal.Round(request.Methods.Sum(x => x.Amount), 2);
                if (methodsSum == request.Amount)
                {
                    request.Status = (byte)BusinessDocumentStatus.PendingDelivery;
                }
                else if (request.Status == (byte)BusinessDocumentStatus.PendingDelivery && methodsSum != request.Amount)
                {
                    request.Status = (byte)BusinessDocumentStatus.Emitted;
                }
            }

            await db.UpdateAsync((Domain.Financials.PaymentDocument)request);
            await SaveCollections(db, request);
            return await GetPaymentDocument(db, request.Id, true);
        }

        private async Task SaveCollections(IDbConnection db, Api.PostPaymentDocumentRequest request)
        {
            // PaymentDocumentItems
            var currentItems = await db.SelectAsync<Domain.Financials.PaymentDocumentItem>(w => w.PaymentDocumentId == request.Id);
            var itemIdsToDelete = currentItems.Select(x => x.Id).Except(request.Items.Where(w => w.Id != 0).Select(s => s.Id));
            await db.DeleteByIdsAsync<Domain.Financials.PaymentDocumentItem>(itemIdsToDelete);

            var paymentDocument = db.SingleById<Domain.Financials.PaymentDocument>(request.Id);
            paymentDocument.Status = (byte)BusinessDocumentStatus.Delivered;
            await db.UpdateAsync((Domain.Financials.PaymentDocument)paymentDocument);

            foreach (var item in request.Items)
            {
                if (item.Id == 0)
                {
                    item.PaymentDocumentId = request.Id;                   
                    db.Insert(item.ConvertTo<Domain.Financials.PaymentDocumentItem>());
                    if (item.RelatedDocumentId.HasValue)
                    {
                        //Marcar ese documento como cancelado o parcial
                        Domain.Financials.PaymentDocumentLink paymentDocumentLink = new Domain.Financials.PaymentDocumentLink();
                        paymentDocumentLink.PaymentDocumentId = request.Id;
                        paymentDocumentLink.LinkedDocumentTypeId = 6;
                        paymentDocumentLink.LinkedDocumentId = (int)item.RelatedDocumentId;
                        paymentDocumentLink.Id = (int)await db.InsertAsync((Domain.Financials.PaymentDocumentLink)paymentDocumentLink, true);

                        var businessDocument = db.SingleById<BusinessDocument>(item.RelatedDocumentId);

                        businessDocument.Status = BusinessDocumentStatus.Paid;
                        await db.UpdateAsync(businessDocument);
                            
                        var workflowInstance = Db.SingleById<Domain.System.Workflows.WorkflowInstance>(businessDocument.ApprovalWorkflowInstanceId);
                        workflowInstance.CurrentWorkflowActivityId = 2332;
                        workflowInstance.Progress = 85;
                        db.Update(workflowInstance);


                        //Aplicar Capital entre recibo e instrumento de cobro
                        var ProductBaseAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                        var itemCapital = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == item.RelatedDocumentId && x.ProductId == ProductBaseAmountId)).FirstOrDefault();
                        if (itemCapital != null)
                        {
                            Domain.Financials.PaymentDocumentItemLink paymentDocumentItemLink = new Domain.Financials.PaymentDocumentItemLink();
                            paymentDocumentItemLink.PaymentDocumentLinkId = paymentDocumentLink.Id;
                            paymentDocumentItemLink.BusinessDocumentItemId = itemCapital.Id;
                            var capital = (decimal)item.ParentItems.Sum(x => x.CapitalApplication);
                            paymentDocumentItemLink.ApplicationAmount = capital;
                            paymentDocumentItemLink.CreateDate = DateTime.UtcNow;
                            paymentDocumentItemLink.CreatedBy = Session.UserId;
                            paymentDocumentItemLink.Id = (int)await db.InsertAsync(paymentDocumentItemLink, true);

                        }


                        //Aplicar Interes entre recibo e instrumento de cobro
                        //Aplicar Interes entre instrumento de cobro e item de expdiente
                        var ProductInterestAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                        var itemInterest = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == item.RelatedDocumentId && x.ProductId == ProductInterestAmountId)).FirstOrDefault();
                        if (itemInterest != null)
                        {
                            Domain.Financials.PaymentDocumentItemLink paymentDocumentItemLink = new Domain.Financials.PaymentDocumentItemLink();
                            paymentDocumentItemLink.PaymentDocumentLinkId = paymentDocumentLink.Id;
                            paymentDocumentItemLink.BusinessDocumentItemId = itemInterest.Id;
                            var interest = (decimal)item.ParentItems.Sum(x => x.InterestApplication);
                            paymentDocumentItemLink.ApplicationAmount = interest;
                            paymentDocumentItemLink.CreateDate = DateTime.UtcNow;
                            paymentDocumentItemLink.CreatedBy = Session.UserId;
                            paymentDocumentItemLink.Id = (int)await db.InsertAsync(paymentDocumentItemLink, true);
                        }

                        //Aplico directamente al item del expediente
                        foreach (var parentItem in item.ParentItems)
                        {
                            var itemToUpdate = Db.SingleById<BusinessDocumentItem>(parentItem.DocumentItemRelatedId);
                            itemToUpdate.AppliedAmount += (decimal)parentItem.CapitalApplication;
                            itemToUpdate.AppliedInterest += (decimal)(parentItem.InterestApplication ?? 0);
                            itemToUpdate.PendingInterest = (decimal)(parentItem.AmountInterest - (parentItem.InterestApplication??0));
                            itemToUpdate.VoidDate = paymentDocument.DocumentDate;

                            if (itemToUpdate.AppliedAmount >= itemToUpdate.UnitPrice)
                            {
                                if ((itemToUpdate.PendingInterest??0) > 0)
                                {
                                    itemToUpdate.UnitPrice = itemToUpdate.PendingInterest??0;
                                    itemToUpdate.PendingInterest = 0;
                                    itemToUpdate.AppliedAmount = 0;
                                }
                            }
                            Db.Update(itemToUpdate);

                            //Aplicar Capital entre instrumento de cobro e item de expdiente
                            if (itemCapital != null)
                            {
                                var paymentCouponItemLinkToParent = Db.Select(Db.From<BusinessDocumentItemLink>().Where(x => x.DocumentItemId == itemCapital.Id && x.DocumentItemRelatedId == parentItem.DocumentItemRelatedId)).FirstOrDefault();
                                if (paymentCouponItemLinkToParent != null)
                                {
                                    paymentCouponItemLinkToParent.AppliedAmount += parentItem.CapitalApplication;
                                    paymentCouponItemLinkToParent.Amount = (double)(itemToUpdate.UnitPrice - itemToUpdate.AppliedAmount + (decimal)parentItem.CapitalApplication); 
                                    
                                    await Db.UpdateAsync(paymentCouponItemLinkToParent);
                                }
                            }
                            //Aplicar Interes entre instrumento de cobro e item de expdiente
                            if (itemInterest != null)
                            {
                                var paymentCouponItemLinkToParent = Db.Select(Db.From<BusinessDocumentItemLink>().Where(x => x.DocumentItemId == itemInterest.Id && x.DocumentItemRelatedId == parentItem.DocumentItemRelatedId)).FirstOrDefault();
                                if (paymentCouponItemLinkToParent != null)
                                {
                                    paymentCouponItemLinkToParent.AppliedAmount += parentItem.InterestApplication;
                                    await Db.UpdateAsync(paymentCouponItemLinkToParent);
                                }
                            }
                        }



                        //var parentDocumentId = db.Select(Db.From<BusinessDocument>().Join<BusinessDocument, BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.LinkedDocumentId == item.RelatedDocumentId && bd.TypeId == 24)).Select(x => x.Id).FirstOrDefault();
                        //var itemsToSetPaid = Db.Select(Db.From<BusinessDocumentItemLink>().Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.BusinessDocumentId == parentDocumentId && bdil.DocumentItemId == bdi.Id)).Select(x => x.DocumentItemRelatedId).Distinct();
                        //foreach (var itemToSetPaid in itemsToSetPaid)
                        //{
                        //    var itemToUpdate = Db.SingleById<BusinessDocumentItem>(itemToSetPaid);
                        //    itemToUpdate.AppliedAmount = itemToUpdate.UnitPrice;
                        //    db.Update(itemToUpdate);
                        //}


                        //var currentActivity = Db.SingleById<Domain.System.Workflows.WorkflowActivity>(workflowInstance.CurrentWorkflowActivityId);
                        //var workflowActivities = Db.Select(Db.From<Domain.System.Workflows.WorkflowActivity>().Where(x => x.WorkflowId == workflowInstance.WorkflowId && x.ListIndex > currentActivity.ListIndex));
                        //foreach (var workflowActivity in workflowActivities)
                        //{

                        //}


                    }
                }
                else
                {
                    db.Update(item);
                }
            }

            // PaymentDocumentMethods
            var currentMethods = await db.SelectAsync<Domain.Financials.PaymentDocumentMethod>(w => w.IssuerPaymentDocumentId == request.Id);
            var idsToDelete = currentMethods.Select(x => x.Id).Except(request.Methods.Where(w => w.Id != 0).Select(s => s.Id)).ToList();
            await db.DeleteByIdsAsync<Domain.Financials.PaymentDocumentMethod>(idsToDelete);

            foreach (var item in request.Methods)
            {
                if (item.Id == 0)
                {
                    item.IssuerPaymentDocumentId = request.Id;
                    await db.InsertAsync(item);
                }
                else
                {
                    await db.UpdateAsync(item);
                }
            }

            //AccountEntry
            /*
            var account = db.SingleById<Domain.BusinessPartners.BusinessPartnerAccount>(request.AccountId);//.ConvertTo<Api.GetPaymentDocumentResponse>();
            if (account != null)
            {
                var accountEntry = db.Select(
                                        db.From<Domain.BusinessPartners.BusinessPartnerAccountEntry>()
                                            .Where(w => w.LinkedDocumentTypeId == request.TypeId &&
                                            w.LinkedDocumentId == request.Id && w.AccountId == account.Id))
                                    .SingleOrDefault();

                var amount = request.Amount; // currentItems.Sum(x => x.AmountToPay);
                var description = "Orden de Pago nro. " + request.Number;
                if (accountEntry == null)
                {
                    accountEntry = new BusinessPartnerAccountEntry
                    {
                        AccountId = account.Id,
                        Amount = amount,
                        Description = description,
                        CreateDate = DateTime.UtcNow,
                        LinkedDocumentId = request.Id,
                        LinkedDocumentTypeId = (short)request.TypeId,
                        PostingDate = DateTime.UtcNow
                    };
                    db.Insert(accountEntry);
                }
                else
                {
                    accountEntry.Amount = amount;
                    accountEntry.Description = description;
                    db.Update(accountEntry);
                }
            }*/
        }
    }
}