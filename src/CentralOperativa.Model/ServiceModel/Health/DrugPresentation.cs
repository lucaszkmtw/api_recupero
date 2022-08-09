using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class DrugPresentation
    {
        [Route("/health/drugpresentations/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/drugpresentations", "POST")]
        [Route("/health/drugpresentations/{Id}", "PUT")]
        public class Post : Domain.Health.DrugPresentation
        {
        }

        [Route("/health/drugpresentations", "GET")]
        public class Query : QueryDb<Domain.Health.DrugPresentation, QueryResult>
        {
        }

        [Route("/health/drugpresentations/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
