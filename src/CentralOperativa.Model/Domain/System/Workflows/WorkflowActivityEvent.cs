using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivityEvents")]
    public class WorkflowActivityEvent
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowActivity))]
        public int WorkflowActivityId { get; set; }

        [References(typeof(WorkflowEvent))]
        public int WorkflowEventId { get; set; }
    }
}