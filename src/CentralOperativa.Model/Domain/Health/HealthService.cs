using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("HealthServices")]
    public class HealthService
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}