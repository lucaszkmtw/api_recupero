using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("Workflows")]
    public class Workflow
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowType))]
        public short TypeId { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}