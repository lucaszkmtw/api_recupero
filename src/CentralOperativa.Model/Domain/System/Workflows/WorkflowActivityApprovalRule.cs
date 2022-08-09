using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivityApprovalRules")]
    public class WorkflowActivityApprovalRule
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowActivity))]
        public int WorkflowActivityId { get; set; }

        [References(typeof(Role))]
        public int? UserId { get; set; }

        [References(typeof(Role))]
        public int? RoleId { get; set; }
    }
}