using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/proxies/{Id}", "GET")]
    public class GetProxie
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/proxies", "POST")]
    [Route("/financials/debtmanagement/proxies/{Id}", "PUT")]
    public class PostProxie : Proxie
    {
    }

    [Route("/financials/debtmanagement/proxies/{Id}", "DELETE")]
    public class DeleteProxie : Proxie
    {
    }

    [Route("/financials/debtmanagement/proxies", "GET")]
    public class QueryProxies : QueryDb<Proxie, QueryProxieResult>
       // , IJoin<Domain.Financials.DebtManagement.Proxie, OrganismType>
        , IJoin<Domain.Financials.DebtManagement.Proxie, Person>
    {
    }

    public class QueryProxieResult
    {
        public int Id { get; set; }     
        public string PersonName { get; set; }
    }

    [Route("/financials/debtmanagement/proxies/lookup", "GET")]
    public class LookupProxie : LookupRequest, IReturn<List<LookupItem>>
    {
        //public string BusinessPartnerTypeName { get; set; }
    }
  
    public class GetProxieResult : Proxie
    {
        public string PersonName { get; set; }
    }

    public class Proxie : Domain.Financials.DebtManagement.Proxie
    {
        public Proxie()
        {
        }
    }
   

}
