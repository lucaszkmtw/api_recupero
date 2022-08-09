using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Persons
{
    public class SectorType
    {
        [Route("/organization/sectortypes/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/organization/sectortypes/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/organization/sectortypes/{Id}", "PUT")]
        public class Put : Domain.System.Persons.SectorType
        {
        }

        [Route("/organization/sectortypes", "POST")]
        public class Post : Domain.System.Persons.SectorType
        {
        }

        [Route("/organization/sectortypes", "GET")]
        public class Find
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Code { get; set; }
        }
    }
}
