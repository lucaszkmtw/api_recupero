using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("ClaimProcesses")]
    public class ClaimProcess
    {
        public int ClaimId { get; set; }

        public int Id { get; set; }

        public int WorkflowInstanceId { get; set; }

        public DateTime CreateDate { get; set; }

        public string WorkflowName { get; set; }

        public string WorkflowCode { get; set; }

        public string WorkflowDescription { get; set; }

        public int PersonId { get; set; }

        public string PersonName { get; set; }
    }
}