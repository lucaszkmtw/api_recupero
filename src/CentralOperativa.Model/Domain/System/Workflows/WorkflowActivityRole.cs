using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivityRoles")]
    public class WorkflowActivityRole
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowActivity))]
        public int WorkflowActivityId { get; set; }

        [References(typeof(Role))]
        public int RoleId { get; set; }

        public bool IsDefault { get; set; }
    }
}