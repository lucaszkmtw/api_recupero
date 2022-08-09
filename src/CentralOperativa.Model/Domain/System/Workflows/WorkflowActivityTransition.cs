using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivitiesTransitions")]
    public class WorkflowActivityTransition
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Workflow))]
        public int WorkflowId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int FromWorkflowActivityId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int ToWorkflowActivityId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}