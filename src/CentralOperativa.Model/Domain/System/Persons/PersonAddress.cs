using CentralOperativa.Domain.System.Location;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonAddresses")]
     public class PersonAddress
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(Address))]
        public int AddressId { get; set; }

        [References(typeof(AddressType))]
        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public bool IsDefault { get; set; }
    }
}
