using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/traders/{Id}", "GET")]
    public class GetTrader
    {
        public int Id { get; set; }
    }

    [Route("/investments/traders", "POST")]
    [Route("/investments/traders/{Id}", "PUT")]
    public class PostTrader : Domain.Investments.Trader
    {

    }

    [Route("/investments/traders", "GET")]
    public class QueryTraders : QueryDb<Domain.Investments.Trader, QueryTradersResult>
        , IJoin<Domain.Investments.Trader, Person>
        , IJoin<Domain.Investments.Trader, Domain.BusinessPartners.BusinessPartner>
    {
    }

    [Route("/investments/traders/lookup", "GET")]
    public class LookupTraders : LookupRequest, IReturn<List<LookupItem>>
    {
    }


    public class QueryTradersResult
    {
        public int Id { get; set; }
        public string Commission { get; set; }
        public string PersonName { get; set; }
        public int BusinessPartnerStatus { get; set; }

    }

    public class Trader : Domain.Investments.Trader
    {
        public Trader()
        {           
        }
    }
    [Route("/investments/traders/{Id}", "DELETE")]
    public class DeleteTrader :  Domain.Investments.Trader
    {
    }

    public class GetTraderResult : Domain.Investments.Trader
    {
        public string PersonName { get; set; }
    }
}