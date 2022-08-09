using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Procurement
{
    public class DistributionFactor
    {
        [Route("/procurement/distributionfactors/{Id}", "GET")]
        public class GetDistributionFactor
        {
            public int Id { get; set; }
        }

        [Route("/procurement/distributionfactors", "GET")]
        public class QueryDistributionFactors : QueryDb<Domain.Procurement.DistributionFactor, QueryDistributionFactorsResult>
        {
        }

        [Route("/procurement/distributionfactors", "POST")]
        [Route("/procurement/distributionfactors/{Id}", "PUT")]
        public class Post : Domain.Procurement.DistributionFactor
        {
        }

        public class QueryDistributionFactorsResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
        }

        [Route("/procurement/distributionfactors/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
