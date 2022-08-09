using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
        [Route("/financials/debtmanagement/debtortypes/{Id}", "GET")]
        public class GetDebtorType
        {
            public int Id { get; set; }
        }

        [Route("/financials/debtmanagement/debtortypes", "POST")]
        [Route("/financials/debtmanagement/debtortypes/{Id}", "PUT")]
        public class PostDebtortype : DebtorType
        {
        }

        [Route("/financials/debtmanagement/debtortypes/{Id}", "DELETE")]
        public class DeleteDebtorType : DebtorType
        {
        }

        [Route("/financials/debtmanagement/debtortypes", "GET")]
        /*
        public class QueryDebtorTypes
        {
            public int? Skip { get; set; }
            public int? Take { get; set; }
            public string Name { get; set; }
		    public int BusinessPartnerId { get; set; }

		//public string Code { get; set; }
 	    }
	    */

	    public class QueryDebtorTypes : QueryDb <DebtorType, QueryDebtorTypeResult>
		//, IJoin<Domain.Financials.DebtManagement.DebtorType, DebtorType>
		, IJoin<DebtorType, BusinessPartnerType>

	    {
	    }
	
	    public class QueryDebtorTypeResult
	    {
		public int Id { get; set; }
		public string Name { get; set; }
		public int BusinessPartnerTypeId { get; set; }
		public string BusinessPartnerTypeName { get; set; }
	    }

	    /*
	    public class GetDebtorTypeResult : BusinessPartnerType
	    {
		public string BussinesPartnerTypeName { get; set; }
	    }
	    */

        [Route("/financials/debtmanagement/debtortypes/lookup", "GET")]
        public class LookupDebtorType : LookupRequest, IReturn<List<LookupItem>>
        {
        }
	
}
