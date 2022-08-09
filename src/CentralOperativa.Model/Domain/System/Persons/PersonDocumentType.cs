using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonDocumentTypes")]
     public class PersonDocumentType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int? TenantId { get; set; }

        public string Name { get; set; }
    }
}
