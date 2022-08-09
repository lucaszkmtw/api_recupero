using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("Specialities")]
    public class Speciality
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}