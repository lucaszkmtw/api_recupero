using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonEmails")]
     public class PersonEmail
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(EmailType))]
        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public string Address { get; set; }
    }
}
