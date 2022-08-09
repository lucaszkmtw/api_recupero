using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonRelationshipTypes")]
    public class PersonRelationshipType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name{ get; set; }
    }
}
