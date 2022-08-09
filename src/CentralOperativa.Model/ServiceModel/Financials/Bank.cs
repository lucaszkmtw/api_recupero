using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    public class Bank
    {
        [Route("/financials/banks/{Id}", "GET")]
        public class GetBank
        {
            public int Id { get; set; }
        }

        [Route("/financials/banks", "POST")]
        [Route("/financials/banks/{Id}", "PUT")]
        public class PostBank : Domain.Financials.Bank
        {
        }
        [Route("/financials/banks", "GET")]
        public class QueryBanks
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        [Route("/financials/banks/lookup", "GET")]
        public class LookupBank : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}