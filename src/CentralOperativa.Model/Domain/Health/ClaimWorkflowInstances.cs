using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("ClaimWorkflowInstances")]
    public class ClaimWorkflowInstance
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Claim))]
        public int ClaimId { get; set; }

        [References(typeof(System.Workflows.WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }
    }
}