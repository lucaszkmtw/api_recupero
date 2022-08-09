using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("ClaimPersons")]
    public class ClaimPerson
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Claim))]
        public int ClaimId { get; set; }

        [References(typeof(System.Persons.Person))]
        public int PersonId { get; set; }
    }
}