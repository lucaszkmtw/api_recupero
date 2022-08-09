using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class Drug
    {
        [Route("/health/drugs/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/drugs", "POST")]
        [Route("/health/drugs/{Id}", "PUT")]
        public class Post : Domain.Health.Drug
        {
        }

        [Route("/health/drugs", "GET")]
        public class Query : QueryDb<Domain.Health.Drug, QueryResult>
        {
        }

        [Route("/health/drugs/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Action { get; set; }
        }
    }
}
