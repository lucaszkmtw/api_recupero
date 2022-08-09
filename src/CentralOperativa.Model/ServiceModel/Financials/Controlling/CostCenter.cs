using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials.Controlling
{
    [Route("/financials/controlling/costcenters/{Id}", "GET")]
    public class GetCostCenter : IReturn<CostCenter>
    {
        public int Id { get; set; }
    }

    [Route("/financials/controlling/costcenters", "POST")]
    [Route("/financials/controlling/costcenters/{Id}", "PUT")]
    public class PostCostCenter : CostCenter, IReturn<CostCenter>
    {
    }

    public class CostCenter
    {
        public int Id { get; set; }

        public short CurrenctyId { get; set; }

        public string Name { get; set; }
    }

    [Route("/financials/controlling/costcenters", "GET")]
    public class QueryCostCenters : QueryDb<Domain.Financials.Controlling.CostCenter, QueryCostCentersResult>
    {
    }

    public class QueryCostCentersResult
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [Route("/financials/controlling/costcenters/lookup", "GET")]
    public class LookupCostCenter : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/financials/controlling/costcenters/{Id}", "DELETE")]
    public class DeleteCostCenter
    {
        public int Id { get; set; }
    }
}