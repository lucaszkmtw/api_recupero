using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class CommercialDrug
    {
        [Route("/health/commercialdrugs/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/health/commercialdrugs", "POST")]
        [Route("/health/commercialdrugs/{Id}", "PUT")]
        public class Post : Domain.Health.CommercialDrug
        {
        }

        [Route("/health/commercialdrugs", "GET")]
        public class Query : QueryDb<Domain.Health.CommercialDrug, QueryResult>,
            IJoin<Domain.Health.CommercialDrug, Domain.Health.Drug>,
            ILeftJoin<Domain.Health.CommercialDrug, Domain.Health.DrugPresentation>
        {
        }

        [Route("/health/commercialdrugs/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public int DrugId { get; set; }
            public string DrugName { get; set; }
            public string DrugPresentationName { get; set; }
            public string PersonName { get; set; }
            public string Name { get; set; }
            public decimal? Price { get; set; }
            public DateTime? PriceValidity { get; set; }
        }
    }
}
