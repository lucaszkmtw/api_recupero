using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.BusinessDocuments
{
    public class BusinessDocumentMessage
    {
        [Route("/businessdocuments/document/{BusinessDocumentId}/messages/{Id}", "PUT")]
        [Route("/businessdocuments/document/{BusinessDocumentId}/messages", "POST")]
        public class Post : Domain.System.Messages.Message
        {
            public int BusinessDocumentId { get; set; }
        }


        [Route("/businessdocuments/document/{BusinessDocumentId}/messages/{Id}", "GET")]
        public class Get : IReturn<GetResponse>
        {
            public int BusinessDocumentId { get; set; }
            public int Id { get; set; }
        }


        [Route("/businessdocuments/document/{BusinessDocumentId}/messages", "GET")]
        public class Query : QueryDb<Domain.System.Messages.Message, ServiceModel.System.Messages.Message.QueryResult>
        {
            public int BusinessDocumentId { get; set; }
        }

        [Route("/businessdocuments/document/{BusinessDocumentId}/messages/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string PersonName { get; set; }
            public string WorkflowCode { get; set; }
            public string WorkflowActivityName { get; set; }
            public string RoleName { get; set; }
            public decimal WorkflowInstanceProgress { get; set; }
        }

        public class GetResponse : Domain.System.Messages.Message
        {
        }
    }
}
