using System.Collections.Generic;

using ServiceStack;

namespace CentralOperativa.ServiceModel.Projects
{
    [Route("/projects/{ProjectId}/costcenters", "GET")]
    public class GetProjectCostCenters : IReturn<List<ProjectCostCenter>>
    {
        public int ProjectId { get; set; }
    }

    [Route("/projects/{ProjectId}/costcenters", "POST")]
    public class PostProjectCostCenter : IReturn<ProjectCostCenter>
    {
        public int ProjectId { get; set; }

        public int CostCenterId { get; set; }
    }

    public class ProjectCostCenter
    {
        public int Id { get; set; }

        public Financials.Controlling.CostCenter CostCenter { get; set; }
    }

    [Route("/projects/{ProjectId}/costcenters/{Id}", "DELETE")]
    public class DeleteProjectCostCenter
    {
        public int ProjectId { get; set; }
        public int Id { get; set; }
    }
}