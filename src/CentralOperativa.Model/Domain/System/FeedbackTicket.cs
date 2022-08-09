using System;
using CentralOperativa.Domain.System.Messages;
using CentralOperativa.Domain.System.Workflows;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("FeedbackTickets")]
    public class FeedbackTicket
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int CreatedByUserId { get; set; }

        public DateTime CreateDate { get; set; }

        [References(typeof(MessageThread))]
        public int MessageThreadId { get; set; }

        [References(typeof(WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        public byte Status { get; set; }
    }
}
