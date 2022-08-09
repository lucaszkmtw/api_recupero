using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class HealthService
    {
        [Route("/system/healthservices/code/{Code}", "GET")]
        public class GetHealthServiceByCode : IReturn<GetResult>
        {
            public string Code { get; set; }
        }

        [Route("/health/healthservices/{Id}", "GET")]
        public class GetHealthService
        {
            public int Id { get; set; }
        }

        public class GetResult : Domain.Health.HealthService
        {
            public Domain.System.Persons.Person Person { get; set; }
        }

        [Route("/health/healthservices", "POST")]
        [Route("/health/healthservices/{Id}", "PUT")]
        public class PostHealthService : Domain.Health.HealthService
        {
            public System.Persons.PostPerson Person { get; set; }
        }

        [Route("/health/healthservices", "GET")]
        public class QueryHealthServices : QueryDb<Domain.Health.HealthService, QueryHealthServicesResponse>, IJoin<Domain.Health.HealthService, Domain.System.Persons.Person>
        {
        }

        [Route("/health/healthservices/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryHealthServicesResponse
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Code { get; set; }
            public string PersonName { get; set; }
        }
    }
}
