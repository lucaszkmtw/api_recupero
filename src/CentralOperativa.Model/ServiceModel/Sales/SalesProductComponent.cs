using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Sales
{
    public class ProductComponent
    {

        [Route("/sales/productcomponents/lookup", "GET")]
        public class Lookup : LookupRequest
        {
        }

        [Route("/sales/productcomponents", "GET")]
        public class Find : QueryDb<Domain.Sales.SalesProductComponent, QueryResult>
            , IJoin<Domain.Sales.SalesProductComponent, Domain.Catalog.Product>
            , IJoin<Domain.Sales.SalesProductComponent, Domain.Sales.SalesProductCatalog>
        {
            public string Name { get; set; }
        }

        [Route("/sales/productcomponents/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/sales/productcomponents", "POST")]
        [Route("/sales/productcomponents/{Id}", "POST, PUT")]
        public class Post : Domain.Sales.SalesProductComponent
        {
        }

        [Route("/sales/productcomponents/{Id}", "DELETE")]
        public class Delete : IReturnVoid
        {
            public int Id { get; set; }
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public int ProductCatalogId { get; set; }
            public string SalesProductCatalogName { get; set; }
        }
    }
}