using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DiseaseGroups")]
    public class DiseaseGroup
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}