using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("Projects"), Schema("projects")]
    public class FundingType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(FundingType))]
        public int ParentId { get; set; }

        public string Name { get; set; }
    }
}
