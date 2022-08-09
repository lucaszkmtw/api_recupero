using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowEvents")]
    public class WorkflowEvent
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}