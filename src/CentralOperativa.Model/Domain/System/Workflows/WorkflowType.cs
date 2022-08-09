using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowTypes")]
    public class WorkflowType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Module))]
        public short ModuleId { get; set; }

        public string Name { get; set; }
    }
}
