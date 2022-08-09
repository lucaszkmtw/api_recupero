using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("Categories"), Schema("projects")]
    public class Category
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Tenant))]
        public int TenantId { get; set; }

        public string Name { get; set; }
    }
}
