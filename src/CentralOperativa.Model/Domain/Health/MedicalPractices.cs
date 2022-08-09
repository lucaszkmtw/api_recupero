using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("MedicalPractices")]
    public class MedicalPractice
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Persons.Skill))]
        public int SkillId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}