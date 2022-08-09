using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class DrugTherapeuticEffect
    {
        [Route("/health/drugtherapeuticeffects/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/drugtherapeuticeffects", "POST")]
        [Route("/health/drugtherapeuticeffects/{Id}", "PUT")]
        public class Post : Domain.Health.DrugTherapeuticEffect
        {
        }

        [Route("/health/drugtherapeuticeffects", "GET")]
        public class Query : QueryDb<Domain.Health.DrugTherapeuticEffect, QueryResult>
        {
        }

        [Route("/health/drugtherapeuticeffects/lookup", "GET")]
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
