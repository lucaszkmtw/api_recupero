using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Emails")]
    public class Email
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }
        
        public int WorkGroupId { get; set; }

        public string Address { get; set; }
    }
}
