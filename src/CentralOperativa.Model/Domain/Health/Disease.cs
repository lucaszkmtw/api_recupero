using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("Diseases")]
    public class Disease
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        [References(typeof(DiseaseFamily))]
        public int FamilyId { get; set; }

        [References(typeof(DiseaseGroup))]
        public int GroupId { get; set; }

        public string Name { get; set; }
    }
}