using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DiseaseFamilies")]
    public class DiseaseFamily
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}