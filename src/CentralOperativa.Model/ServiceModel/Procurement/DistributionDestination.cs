using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Procurement
{
    public class DistributionDestination
    {
        [Route("/procurement/distributiondestinations/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/procurement/distributiondestinations", "POST")]
        [Route("/procurement/distributiondestinations/{Id}", "PUT")]
        public class Post : Domain.Procurement.DistributionDestination
        {
        }
        [Route("/procurement/distributiondestinations", "GET")]
        public class Find
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Description { get; set; }
        }

        [Route("/procurement/distributiondestinations/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
