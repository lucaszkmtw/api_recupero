using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class DrugUnitType
    {
        [Route("/health/drugunittypes/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/drugunittypes", "POST")]
        [Route("/health/drugunittypes/{Id}", "PUT")]
        public class Post : Domain.Health.DrugUnitType
        {
        }

        [Route("/health/drugunittypes", "GET")]
        public class Query : QueryDb<Domain.Health.DrugUnitType, QueryResult>
        {
        }

        [Route("/health/drugunittypes/lookup", "GET")]
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
