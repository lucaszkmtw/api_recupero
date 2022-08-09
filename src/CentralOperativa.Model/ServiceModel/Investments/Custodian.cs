using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/custodians/{Id}", "GET")]
    public class GetCustodian
    {
        public int Id { get; set; }

    }

    [Route("/investments/custodians", "POST")]
    [Route("/investments/custodians/{Id}", "PUT")]
    public class PostCustodian : Domain.Investments.Custodian
    {

    }

    [Route("/investments/custodians", "GET")]
    public class QueryCustodians : QueryDb<Domain.Investments.Custodian, QueryCustodiansResult>
        , IJoin<Domain.Investments.Custodian, Person>
        , IJoin<Domain.Investments.Custodian, Domain.BusinessPartners.BusinessPartner>
    {
    }

    [Route("/investments/custodians/lookup", "GET")]
    public class LookupCustodians : LookupRequest, IReturn<List<LookupItem>>
    {
    }


    public class QueryCustodiansResult
    {
        public int Id { get; set; }
        public string Commission { get; set; }
        public string PersonName { get; set; }

    }

    public class Custodian : Domain.Investments.Custodian
    {
        public Custodian()
        {
        }
    }

    [Route("/investments/custodians/{Id}", "DELETE")]
    public class DeleteCustodian : Domain.Investments.Custodian
    {
    }

    public class GetCustodianResult : Domain.Investments.Custodian
    {
        public string PersonName { get; set; }
    }
}