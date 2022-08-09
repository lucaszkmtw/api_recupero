using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("Claims")]
    public class Claim
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        [References(typeof(Domain.System.Workflows.WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        [References(typeof(Domain.System.Messages.MessageThread))]
        public int MessageThreadId { get; set; }

        [References(typeof(Domain.System.DocumentManagement.Folder))]
        public int? FolderId { get; set; }
    }
}