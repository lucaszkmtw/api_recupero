using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("Workflowtags")]
    public class WorkflowTag
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Workflow))]
        public int? WorkflowId { get; set; }

        [References(typeof(WorkflowTag))]
        public int? ParentId { get; set; }

        public string Name { get; set; }

    }
}
