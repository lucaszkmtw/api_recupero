using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Procurement
{
    public class DistributionCondition
    {
        [Route("/procurement/distributionconditions/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/procurement/distributionconditions", "POST")]
        [Route("/procurement/distributionconditions/{Id}", "PUT")]
        public class Post : Domain.Procurement.DistributionCondition
        {
        }

        [Route("/procurement/distributionconditions", "GET")]
        public class Query : QueryDb<Domain.Procurement.DistributionCondition, QueryResult>
            , IJoin<Domain.Procurement.DistributionCondition, Domain.Procurement.DistributionType>
            , IJoin<Domain.Procurement.DistributionCondition, Domain.Procurement.DistributionDestination>
        {
        }

        [Route("/procurement/distributionconditions/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string Condition { get; set; }
            public string DistributionTypeCode { get; set; }
            public string DistributionDestinationCode { get; set; }
        }
    }
}
