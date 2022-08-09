using System;
using CentralOperativa.Domain.System.Workflows;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Messages
{
    [Alias("Messages")]
    public class Message
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Workflow))]
        public int MessageThreadId { get; set; }

        [References(typeof(Message))]
        public int? ReplyToMessageId { get; set; }

        [References(typeof(User))]
        public int SenderId { get; set; }

        public string Body { get; set; }

        public DateTime CreateDate { get; set; }
    }
}