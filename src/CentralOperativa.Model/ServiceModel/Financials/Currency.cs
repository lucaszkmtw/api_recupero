using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/currencies/{Id}", "GET")]
    public class GetCurrency : IReturn<Domain.Financials.Currency>
    {
        public int Id { get; set; }
    }

    [Route("/financials/currencies", "POST")]
    [Route("/financials/currencies/{Id}", "PUT")]
    public class PostCurrency : Domain.Financials.Currency
    {
    }
    [Route("/financials/currencies", "GET")]
    public class QueryCurrencies
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }

    [Route("/financials/currencies/lookup", "GET")]
    public class LookupCurrency : LookupRequest, IReturn<List<LookupItem>>
    {
    }
}