using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Skills")]
    public class Skill
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}