using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowActivities")]
    public class WorkflowActivity
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Workflow))]
        public int WorkflowId { get; set; }

        public string Name { get; set; }

        public bool IsStart { get; set; }

        public bool IsFinal { get; set; }

        public int ListIndex { get; set; }
    }
}