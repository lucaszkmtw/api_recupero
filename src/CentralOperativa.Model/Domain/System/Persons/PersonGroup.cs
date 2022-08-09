using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonGroups")]
     public class PersonGroup
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(Group))]
        public int GroupId { get; set; }
    }
}
