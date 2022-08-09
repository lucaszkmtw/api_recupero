using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonDocuments")]
     public class PersonDocument
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(PersonDocumentType))]
        public int TypeId { get; set; }

        public string Number { get; set; }

        public string Data { get; set; }
    }
}
