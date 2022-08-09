using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class ClaimType
    {
        [Route("/health/claimtypes/{Id}", "GET")]
        public class Get : IReturn<GetResponse>
        {
            public int Id { get; set; }
        }

        [Route("/health/claimtypes", "POST")]
        [Route("/health/claimtypes/{Id}", "PUT")]
        public class Post : Domain.Health.ClaimType
        {
        }

        [Route("/health/claimtypes", "GET")]
        public class Query : QueryDb<Domain.Health.ClaimType, QueryResult>
        {
            public byte View { get; set; }
            public string Q { get; set; }
        }

        [Route("/health/claimtypes/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
            , IJoin<Domain.Health.Claim, Domain.System.Messages.MessageThread>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<QueryResult> Children { get; set; }
        }

        public class GetResponse : Domain.Health.ClaimType
        {
        }
    }
}
