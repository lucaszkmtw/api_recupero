using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DrugPresentations")]
    public class DrugPresentation
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}