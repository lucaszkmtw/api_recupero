using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Groups")]
    public class Group
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}