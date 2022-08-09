using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Persons
{
    public class Sector
    {
        [Route("/organization/sectors/{Id}", "GET")]
        public class GetSector: IReturn<GetSectorResult>
        {
            public int Id { get; set; }
        }

        [Route("/organization/sectors/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/organization/sectors/{Id}", "PUT")]
        public class Put : Domain.System.Persons.Sector
        {
            public List<int> SectorTypeIds { get; set; }
        }

        [Route("/organization/sectors", "POST")]
        public class Post : Domain.System.Persons.Sector
        {
            public List<int> SectorTypeIds { get; set; }
        }

        [Route("/organization/sectors", "GET")]
        public class Query : QueryDb<Domain.System.Persons.Sector, QueryResult>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public bool ConfigPayrollDistribution { get; set; }
            public string SectorCode { get; set; }
        }

    public class GetSectorResult: Domain.System.Persons.Sector
    {
        public List<int> SectorTypeIds { get; set; }

        public GetSectorResult()
        {
            this.SectorTypeIds = new List<int>();
        }
    }
}
}
