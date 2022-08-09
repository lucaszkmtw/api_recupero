using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Sales
{
    public class PaymentCondition
    {
        [Route("/sales/paymentconditions/lookup", "GET")]
        public class Lookup : LookupRequest
        {
        }

        [Route("/sales/paymentconditions", "GET")]
        public class Find : QueryDb<Domain.Sales.PaymentCondition, QueryResult>
            , IJoin<Domain.Sales.PaymentCondition, Domain.Sales.SalesProductCatalog>
        {
            public string Name { get; set; }
        }

        [Route("/sales/paymentconditions/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/sales/paymentconditions", "POST")]
        [Route("/sales/paymentconditions/{Id}", "POST, PUT")]
        public class Post : Domain.Sales.PaymentCondition
        {
        }

        [Route("/sales/paymentconditions/{Id}", "DELETE")]
        public class Delete : IReturnVoid
        {
            public int Id { get; set; }
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SalesProductCatalogName { get; set; }
        }
    }
}