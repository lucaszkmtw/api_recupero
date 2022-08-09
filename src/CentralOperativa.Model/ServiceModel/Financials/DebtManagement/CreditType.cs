using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.ServiceModel.Catalog;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
        [Route("/financials/debtmanagement/credittypes/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/financials/debtmanagement/credittypes", "POST")]
        [Route("/financials/debtmanagement/credittypes/{Id}", "PUT")]
        public class PostCredittype : Domain.Financials.DebtManagement.CreditType
        {
        }

        [Route("/financials/debtmanagement/credittypes/{Id}", "DELETE")]
        public class DeleteCreditType : Domain.Financials.DebtManagement.CreditType
        {
        }

        [Route("/financials/debtmanagement/credittypes", "GET")]
        public class QueryCreditTypes
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        [Route("/financials/debtmanagement/credittypes/lookup", "GET")]
        public class LookupCreditType : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/financials/debtmanagement/credittypes", "GET")]
        public class Query : QueryDb<Domain.Financials.DebtManagement.CreditType, QueryResult>
        {
        }
    
        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
}
