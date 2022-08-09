using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{

	[Route("/financials/debtmanagement/debtors/{Id}", "GET")]
	public class GetDebtor
	{
		public int Id { get; set; }
	}

	[Route("/financials/debtmanagement/debtors", "POST")]
	[Route("/financials/debtmanagement/debtors/{Id}", "PUT")]
	public class PostDebtor : Debtor
	{
	}

	[Route("/financials/debtmanagement/debtors/{Id}", "DELETE")]
	public class DeleteDebtor : Debtor
	{
	}

	[Route("/financials/debtmanagement/debtors", "GET")]
	public class QueryDebtors : QueryDb<Debtor, QueryDebtorResult>
		, IJoin<Domain.Financials.DebtManagement.Debtor, DebtorType>
		, IJoin<Domain.Financials.DebtManagement.Debtor, Person>
	{
	}

	
	public class QueryDebtorResult
	{
		public int Id { get; set; }
		public int DebtorTypeId { get; set; }
		public int PersonId { get; set; }
		public string DebtorTypeName { get; set; }
		public string PersonName { get; set; }
        public int AccountId { get; set; }
	}


	public class GetDebtorResult : Person
	{
		public string PersonName { get; set; }
	}

	[Route("/financials/debtmanagement/debtors/lookup", "GET")]
	public class LookupDebtor : LookupRequest, IReturn<List<LookupItem>>
	{
		
	}

	public class Debtor : Domain.Financials.DebtManagement.Debtor
	{
		public Debtor()
		{
		}
	}

}












