using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowInstanceApprovals")]
    public class WorkflowInstanceApproval
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int WorkflowActivityId { get; set; }

        [References(typeof(Role))]
        public int? RoleId { get; set; }

        [References(typeof(User))]
        public int? UserId { get; set; }

        public WorkflowInstanceApprovalStatus Status { get; set; }

        public DateTime? Date { get; set; }

        public DateTime CreateDate { get; set; }
    }

    [EnumAsInt]
    public enum WorkflowInstanceApprovalStatus : byte
    {
        Pending,
        Approved,
        Rejected
    }
}