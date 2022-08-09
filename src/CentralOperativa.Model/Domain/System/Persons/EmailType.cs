using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("EmailTypes")]
    public class EmailType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }


    }
}
