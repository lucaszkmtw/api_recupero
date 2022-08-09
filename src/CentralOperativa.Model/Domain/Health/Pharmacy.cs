using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("Pharmacies")]
    public class Pharmacy
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}