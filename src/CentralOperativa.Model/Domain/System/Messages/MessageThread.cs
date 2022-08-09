using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Messages
{
    [Alias("MessageThreads")]
    public class MessageThread
    {
        [AutoIncrement]
        public int Id { get; set; }

        public DateTime CreateDate { get; set; }
    }
}