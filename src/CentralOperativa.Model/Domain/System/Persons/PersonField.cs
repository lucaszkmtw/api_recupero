using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonFields")]
     public class PersonField
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int? TenantId { get; set; }

        public string Name { get; set; }
    }
}
