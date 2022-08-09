using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivityRolePermissions")]
    public class WorkflowActivityRolePermission
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowActivityRole))]
        public int WorkflowActivityRoleId { get; set; }

        [References(typeof(Role))]
        public int RoleId { get; set; }

        /// <summary>
        /// 0: Viewer - 1: Editor - 2: Supervisor
        /// </summary>
        public byte Permission { get; set; }
    }
}