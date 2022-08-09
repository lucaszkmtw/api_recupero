using System.Collections.Generic;
using ServiceStack;
using System;

namespace CentralOperativa.ServiceModel.System.Messages
{
    public class Message : Domain.System.Messages.Message
    {
        [Route("/system/messages/{MessageThreadId}/messages/{Id}", "GET")]
        public class GetMessage : IReturn<Message>
        {
            public int MessageThreadId { get; set; }
            public int Id { get; set; }
        }


        [Route("/system/messages/{MessageThreadId}/messages", "POST")]
        [Route("/system/messages/{MessageThreadId}/messages/{Id}", "PUT")]
        public class Post : IReturn<Message>
        {
        }

        [Route("/system/messages/{MessageThreadId}/messages", "GET")]
        public class QueryMessages : IReturn<List<Message>>
        {
            public int MessageThreadId { get; set; }
        }

        public class QueryResult
        {
            public QueryResult()
            {
                this.Replies = new List<QueryResult>();
            }

            public int Id { get; set; }
            public int? ReplyToMessageId { get; set; }
            public string Body { get; set; }
            public DateTime CreateDate { get; set; }
            public int SenderId { get; set; }
            public string PersonName { get; set; }

            public List<QueryResult> Replies { get; set; }
        }
    }

    
}
