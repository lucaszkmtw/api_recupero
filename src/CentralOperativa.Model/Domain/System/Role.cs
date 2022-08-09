using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Roles")]
    public class Role
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int TenantId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? ListIndex { get; set; }
    }
}