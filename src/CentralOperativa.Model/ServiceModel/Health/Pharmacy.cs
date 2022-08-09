using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class Pharmacy
    {
        [Route("/health/pharmacies/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/pharmacies", "POST")]
        [Route("/health/pharmacies/{Id}", "PUT")]
        public class Post : Domain.Health.Pharmacy
        {
        }

        [Route("/health/pharmacies", "GET")]
        public class Query : QueryDb<Domain.Health.Pharmacy, QueryResult>
        {
        }

        [Route("/health/pharmacies/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Code { get; set; }
        }
    }
}
