using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Sales
{
    public class Contract
    {
        [Route("/sales/contracts/lookup", "GET")]
        public class Lookup : LookupRequest
        {
        }

        [Route("/sales/contracts", "GET")]
        public class Find
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Name { get; set; }
        }

        [Route("/sales/contracts/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/sales/contracts", "POST")]
        [Route("/sales/contracts/{Id}", "POST, PUT")]
        public class Post : Domain.Sales.SalesContract
        {
        }

        [Route("/sales/contracts/{Id}", "DELETE")]
        public class Delete : IReturnVoid
        {
            public int Id { get; set; }
        }
    }
}