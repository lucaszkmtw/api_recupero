using System;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowInstanceAssignedRole
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}
