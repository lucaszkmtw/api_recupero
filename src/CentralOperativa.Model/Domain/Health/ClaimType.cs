using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("ClaimTypes")]
    public class ClaimType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.Health.ClaimType))]
        public int ParentId { get; set; }

        public string Name { get; set; }
    }
}