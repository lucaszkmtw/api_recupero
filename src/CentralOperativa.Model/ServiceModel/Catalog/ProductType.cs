using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Catalog
{
    [Route("/catalog/producttypes/{Id}", "GET")]
    public class GetProductType
    {
        public int Id { get; set; }
    }

    [Route("/catalog/producttypes", "POST")]
    [Route("/catalog/producttypes/{Id}", "PUT")]
    public class PostProductType : Domain.Catalog.ProductType
    {
    }
    [Route("/catalog/producttypes", "GET")]
    public class QueryProductTypes : QueryDb<Domain.Catalog.ProductType, QueryProductTypesResult>
    {
    }

    [Route("/catalog/producttypes/lookup", "GET")]
    public class LookupProductTypes : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryProductTypesResult
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
