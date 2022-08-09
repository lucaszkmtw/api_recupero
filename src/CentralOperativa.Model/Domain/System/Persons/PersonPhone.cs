using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonPhones")]
    public class PersonPhone
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(PhoneType))]
        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public bool IsDefault { get; set; }

        public string Number { get; set; }
    }
}
