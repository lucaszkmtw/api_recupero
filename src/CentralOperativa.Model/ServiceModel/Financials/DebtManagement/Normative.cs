using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/normatives/{Id}", "GET")]
    public class GetNormative
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/normatives", "POST")]
    [Route("/financials/debtmanagement/normatives/{Id}", "PUT")]
    public class PostNormative : Normative
    {
    }

    [Route("/financials/debtmanagement/normatives/{Id}", "DELETE")]
    public class DeleteNormative : Normative
    {
    }

    [Route("/financials/debtmanagement/normatives", "GET")]
    public class QueryNormatives
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string Name { get; set; }
        public string Observations { get; set; }
    }

    [Route("/financials/debtmanagement/normatives/lookup", "GET")]
    public class LookupNormative : LookupRequest, IReturn<List<LookupItem>>
    {
    }

}
