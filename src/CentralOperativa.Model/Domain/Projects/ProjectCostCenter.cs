using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Financials.Controlling;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectCostCenters"), Schema("projects")]
    public class ProjectCostCenter
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Project))]
        public int ProjectId { get; set; }

        [References(typeof(CostCenter))]
        public int CostCenterId { get; set; }
    }
}
