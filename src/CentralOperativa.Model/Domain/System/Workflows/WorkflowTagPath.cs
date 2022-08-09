using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowtagsPath")]
    public class WorkflowTagPath : Domain.System.Workflows.WorkflowTag
    {
        public string Path { get; set; }

        public int Children { get; set; }
    }
}
