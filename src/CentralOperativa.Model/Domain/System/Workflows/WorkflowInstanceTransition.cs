using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowInstanceTransitions")]
    public class WorkflowInstanceTransition
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int FromWorkflowActivityId { get; set; }

        [References(typeof(WorkflowActivity))]
        public int ToWorkflowActivityId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsTerminated { get; set; }

        public object Data { get; set; }
    }
}