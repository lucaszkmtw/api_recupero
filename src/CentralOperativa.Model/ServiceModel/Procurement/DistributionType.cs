using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Procurement
{
    public class DistributionType
    {
        [Route("/procurement/distributiontypes/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/procurement/distributiontypes", "POST")]
        [Route("/procurement/distributiontypes/{Id}", "PUT")]
        public class Post : CentralOperativa.Domain.Procurement.DistributionType
        {
        }
        [Route("/procurement/distributiontypes", "GET")]
        public class Find
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Description { get; set; }
        }

        [Route("/procurement/distributiontypes/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}