using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowInstanceAssignments")]
    public class WorkflowInstanceAssignments
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int WorkflowActivityId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Role))]
        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }
    }
}