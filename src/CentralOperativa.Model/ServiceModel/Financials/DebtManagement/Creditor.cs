using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{

	[Route("/financials/debtmanagement/creditors/{Id}", "GET")]
	public class GetCreditor
	{
		public int Id { get; set; }
	}

	[Route("/financials/debtmanagement/creditors", "POST")]
	[Route("/financials/debtmanagement/creditors/{Id}", "PUT")]
	public class PostCreditor : Creditor
	{
	}

	[Route("/financials/debtmanagement/creditors/{Id}", "DELETE")]
	public class DeleteCreditor : Creditor
	{
	}

	[Route("/financials/debtmanagement/creditors", "GET")]
	public class QueryCreditors : QueryDb<Creditor, QueryCreditorResult>
		//, IJoin<Domain.Financials.DebtManagement.Creditor, CreditorType>
		, IJoin<Domain.Financials.DebtManagement.Creditor, Person>
	{
	}

	
	public class QueryCreditorResult
	{
		public int Id { get; set; }
		public int PersonId { get; set; }
		public string PersonName { get; set; }
        public int AccountId { get; set; }
	}


	public class GetCreditorResult : Person
	{
		public string PersonName { get; set; }
	}

	[Route("/financials/debtmanagement/creditors/lookup", "GET")]
	public class LookupCreditor : LookupRequest, IReturn<List<LookupItem>>
	{

		
	}

	public class Creditor : Domain.Financials.DebtManagement.Creditor
	{
		public Creditor()
		{
		}
	}

}












