using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowInstances")]
    public class WorkflowInstance
    {
        [AutoIncrement]
        public int Id { get; set; }

        public Guid Guid { get; set; }

        [References(typeof(Workflow))]
        public int WorkflowId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int CurrentWorkflowActivityId { get; set; }

        [References(typeof(User))]
        public int CreatedByUserId { get; set; }

        public DateTime CreateDate { get; set; }

        public decimal Progress { get; set; }

        public bool IsTerminated { get; set; }
    }
}