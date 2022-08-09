using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/investors/{Id}", "GET")]
    public class GetInvestor
    {
        public int Id { get; set; }

    }

    [Route("/investments/investors", "POST")]
    [Route("/investments/investors/{Id}", "PUT")]
    public class PostInvestor : Domain.Investments.Investor
    {

    }

    [Route("/investments/investors", "GET")]
    public class QueryInvestors : QueryDb<Domain.Investments.Investor, QueryInvestorsResult>
        , IJoin<Domain.Investments.Investor, Person>
        , IJoin<Domain.Investments.Investor, Domain.BusinessPartners.BusinessPartner>
    {
    }

    [Route("/investments/investors/lookup", "GET")]
    public class LookupInvestors : LookupRequest, IReturn<List<LookupItem>>
    {
        public string PersonName { get; set; }
        public string Commission { get; set; }
    }


    public class QueryInvestorsResult
    {
        public int Id { get; set; }
        public string Commission { get; set; }
        public string PersonName { get; set; }
       
    }

    public class Investor : Domain.Investments.Investor
    {
        public Investor()
        {           
        }
    }

    [Route("/investments/investors/{Id}", "DELETE")]
    public class DeleteInvestor : Domain.Investments.Investor
    {
    }

    public class GetInvestortResult : Domain.Investments.Investor
    {
        public string PersonName{ get; set; }
    }
}