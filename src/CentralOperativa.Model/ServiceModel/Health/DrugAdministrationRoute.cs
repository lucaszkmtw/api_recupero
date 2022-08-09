using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class DrugAdministrationRoute
    {
        [Route("/health/drugadministrationroutes/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/drugadministrationroutes", "POST")]
        [Route("/health/drugadministrationroutes/{Id}", "PUT")]
        public class Post : Domain.Health.DrugAdministrationRoute
        {
        }

        [Route("/health/drugadministrationroutes", "GET")]
        public class Query : QueryDb<Domain.Health.DrugAdministrationRoute, QueryResult>
        {
        }

        [Route("/health/drugadministrationroutes/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public short Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
