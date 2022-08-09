using System;
using System.Collections.Generic;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using BusinessDocumentMessage = CentralOperativa.ServiceModel.BusinessDocuments.BusinessDocumentMessage;

namespace CentralOperativa.ServiceInterface.BusinessDocuments
{

    public class BusinessDocumentMessageService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Post(BusinessDocumentMessage.Post request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var businessDocument = Db.SingleById<Domain.BusinessDocuments.BusinessDocument>(request.BusinessDocumentId);

                    if (!businessDocument.MessageThreadId.HasValue)
                    {
                        var messageThread = new Domain.System.Messages.MessageThread { CreateDate = DateTime.UtcNow };
                        messageThread.Id = (int)Db.Insert(messageThread, true);
                        businessDocument.MessageThreadId = messageThread.Id;
                        Db.Update(businessDocument);
                    }

                    request.MessageThreadId = businessDocument.MessageThreadId.Value;
                    request.CreateDate = DateTime.UtcNow;
                    request.SenderId = Session.UserId;
                    request.Id = (int)Db.Insert(request.ConvertTo<Domain.System.Messages.Message>(), true);

                    trx.Commit();

                    var businessDocumentMessageService = this.ResolveService<BusinessDocumentMessageService>();

                    var messages = businessDocumentMessageService.Get(new ServiceModel.BusinessDocuments.BusinessDocumentMessage.Query { BusinessDocumentId = businessDocument.Id });
                    return messages;
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }
        }

        public object Put(BusinessDocumentMessage.Post request)
        {
            Db.Update(request.ConvertTo<Domain.System.Messages.Message>());
            return Db.Select<Domain.System.Messages.Message>(x => x.MessageThreadId == request.MessageThreadId);
        }

        public object Get(BusinessDocumentMessage.Get request)
        {
            var treatmentRequestPractice = Db.SingleById<Domain.Health.TreatmentRequestPractice>(request.Id).ConvertTo<BusinessDocumentMessage.GetResponse>();
            return treatmentRequestPractice;
        }

        public List<ServiceModel.System.Messages.Message.QueryResult> Get(BusinessDocumentMessage.Query request)
        {
            // Messages
            var q = Db.From<Domain.System.Messages.Message>()
                .Join<Domain.System.Messages.Message, Domain.System.User>((m, u) => m.SenderId == u.Id)
                .Join<Domain.System.User, Domain.System.Persons.Person>()
                .Join<Domain.System.Messages.Message, Domain.System.Messages.MessageThread>()
                .Join<Domain.System.Messages.MessageThread, CentralOperativa.Domain.BusinessDocuments.BusinessDocument>()
                .Where<CentralOperativa.Domain.BusinessDocuments.BusinessDocument>(x => x.Id == request.BusinessDocumentId);

            var messages = Db.Select<ServiceModel.System.Messages.Message.QueryResult>(q);
            var modelMessages = new List<ServiceModel.System.Messages.Message.QueryResult>();

            foreach (var rootMessage in messages.Where(x => !x.ReplyToMessageId.HasValue).ToList())
            {
                modelMessages.Add(AddMessage(messages, rootMessage));
            }

            return modelMessages;
        }

        private ServiceModel.System.Messages.Message.QueryResult AddMessage(List<ServiceModel.System.Messages.Message.QueryResult> messages, ServiceModel.System.Messages.Message.QueryResult parent)
        {
            parent.ReplyToMessageId = null;
            foreach (var reply in messages.Where(x => x.ReplyToMessageId == parent.Id).ToList())
            {
                parent.Replies.Add(AddMessage(messages, reply));
            }

            return parent;
        }

    }
}