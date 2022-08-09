using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonSkills")]
    public class PersonSkill
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

         [References(typeof(Skill))]
        public int SkillId { get; set; }
    }
}
