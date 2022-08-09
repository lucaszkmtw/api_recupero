using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/managers/{Id}", "GET")]
    public class GetManager
    {
        public int Id { get; set; }

    }

    [Route("/investments/managers", "POST")]
    [Route("/investments/managers/{Id}", "PUT")]
    public class PostManager : Domain.Investments.Manager
    {

    }

    [Route("/investments/managers", "GET")]
    public class QueryManagers : QueryDb<Domain.Investments.Manager, QueryManagersResult>
            , IJoin<Domain.Investments.Manager, Person>
            , IJoin<Domain.Investments.Manager, Domain.BusinessPartners.BusinessPartner>
    {
    }

    [Route("/investments/managers/lookup", "GET")]
    public class LookupManagers : LookupRequest, IReturn<List<LookupItem>>
    {
    }


    public class QueryManagersResult
    {
        public int Id { get; set; }
        public string Commission { get; set; }
        public string PersonName { get; set; }
       
    }

    public class Manager : Domain.Investments.Manager
    {
        public Manager()
        {           
        }
    }

    [Route("/investments/managers/{Id}", "DELETE")]
    public class DeleteManager : Domain.Investments.Manager
    {
    }
    public class GetManagerResult : Domain.Investments.Manager
    {
        public string PersonName { get; set; }
    }
}