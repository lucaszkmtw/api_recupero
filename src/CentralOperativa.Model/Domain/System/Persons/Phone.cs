using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Phones")]
    public class Phone
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Number { get; set; }
    }
}
