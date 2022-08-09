using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Inv
{
    [Alias("InventorySites")]
    public class InventorySite
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        public string Name { get; set; }
    }
}
