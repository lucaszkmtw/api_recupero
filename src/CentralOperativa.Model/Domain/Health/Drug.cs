using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("Drugs")]
    public class Drug
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Action { get; set; }
    }
}