using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonFieldValues")]
     public class PersonFieldValue
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(PersonField))]
        public int FieldId { get; set; }

        public string Value { get; set; }
    }
}
