using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PhoneTypes")]
    public class PhoneType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int Name { get; set; }
    }
}
