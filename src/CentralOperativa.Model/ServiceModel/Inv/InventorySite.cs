using ServiceStack;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Inv
{
    [Route("/inv/sites/", "GET")]
    public class QuerySites : QueryDb<Domain.Inv.InventorySite>
    {

    }

    [Route("/inv/site/{Id}", "GET")]
    public class QuerySite : Domain.Inv.InventorySite
    {

    }

    [Route("/inv/site", "POST")]
    [Route("/inv/site/{Id}", "PUT")]
    public class PostSite : Domain.Inv.InventorySite
    {

    }

    [Route("/inv/sites/lookup", "GET")]
    public class LookupSite : LookupRequest, IReturn<List<LookupItem>>
    {
        public int PersonId { get; set; }
    }

    [Route("/system/persons/{PersonId}/inventorysites", "GET")]
    public class LookupSiteByPerson : LookupRequest, IReturn<List<Domain.Inv.InventorySite>>
    {
        public int PersonId { get; set; }
    }
}
