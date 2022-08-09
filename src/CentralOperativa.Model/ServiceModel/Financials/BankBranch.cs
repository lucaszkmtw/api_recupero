using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    public class BankBranch
    {
        [Route("/financials/bankbranches/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/financials/bankbranches", "POST")]
        [Route("/financials/bankbranches/{Id}", "PUT")]
        public class Post : Domain.Financials.BankBranch
        {
        }
        [Route("/financials/bankbranches", "GET")]
         public class Find : QueryDb<Domain.Financials.BankBranch, QueryResult>
            , IJoin<Domain.Financials.BankBranch, Domain.Financials.Bank>
        {
            public string Name { get; set; }
        }

        [Route("/financials/bankbranches/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string BankName { get; set; }
        }
    }
}