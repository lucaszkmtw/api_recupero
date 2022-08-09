using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/organismcredittypes/{Id}", "GET")]
    public class GetOrganismCreditType
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/organismcredittypes", "POST")]
    [Route("/financials/debtmanagement/organismcredittypes/{Id}", "PUT")]
    public class PostOrganismCreditType : OrganismCreditType
    {
        public List<CreditType> CreditTypes {
			get; set; }
    }

    [Route("/financials/debtmanagement/organismcredittypes/{Id}", "DELETE")]
    public class DeleteOrganismCreditType : OrganismCreditType
    {
    }

    [Route("/financials/debtmanagement/organismcredittypes", "GET")]
    public class QueryOrganismCreditTypes : QueryDb<OrganismCreditType, QueryOrganismCreditTypeResult>
        , IJoin<OrganismCreditType,CreditType>
        , IJoin<OrganismCreditType, Organism>
		, IJoin<Organism, Person>
	{

    }

    public class QueryOrganismCreditTypeResult
    {
        public int Id { get; set; }
        public string CreditTypeName { get; set; }
        public string PersonName { get; set; }
		public int OrganismId { get; set; }
		public int OrganismStatus { get; set; }
		public int OrganismCreditTypeStatus { get; set; }
	}

    [Route("/financials/debtmanagement/organismcredittypes/lookup", "GET")]
    public class LookupOrganismCreditType : LookupRequest, IReturn<List<LookupItem>>
    {
        
    }

    public class GetOrganismCreditTypeResult : OrganismCreditType
    {
        public string PersonName { get; set; }
    }

    public class OrganismCreditType : Domain.Financials.DebtManagement.OrganismCreditType
    {
        public OrganismCreditType()
        {
        }
    }


}
