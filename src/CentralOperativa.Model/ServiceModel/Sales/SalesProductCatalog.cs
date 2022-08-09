using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Sales
{
    public class ProductCatalog
    {
        [Route("/sales/productcatalogs/lookup", "GET")]
        public class Lookup : LookupRequest
        {
        }

        [Route("/sales/productcatalogs", "GET")]
        public class Find
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Name { get; set; }
        }

        [Route("/sales/productcatalogs/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/sales/productcatalogs", "POST")]
        [Route("/sales/productcatalogs/{Id}", "POST, PUT")]
        public class Post : Domain.Sales.SalesProductCatalog
        {
        }

        [Route("/sales/productcatalogs/{Id}", "DELETE")]
        public class Delete : IReturnVoid
        {
            public int Id { get; set; }
        }
    }
}