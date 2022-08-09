using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class Disease
    {
        [Route("/health/diseases/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/diseases", "POST")]
        [Route("/health/diseases/{Id}", "PUT")]
        public class Post : Domain.Health.Disease
        {
        }

        [Route("/health/diseases", "GET")]
        public class Query : QueryDb<Domain.Health.Disease, QueryResult>
        {
        }

        [Route("/health/diseases/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class GetResponse : Domain.Health.Disease
        {
            public string DiseaseFamilyName { get; set; }
            public string DiseaseGroupName { get; set; }
        }
    }
}
