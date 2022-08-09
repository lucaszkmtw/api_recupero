using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonRelationships")]
    public class PersonRelationship
    {
        [AutoIncrement]
        public int Id  { get; set; }

        public int FromPersonId { get; set; }

        public int ToPersonId { get; set; }

        [References(typeof(PersonRelationshipType))]
        public int TypeId { get; set; }
    }
}
