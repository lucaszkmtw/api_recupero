using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.Domain.Financials.DebtManagement
{

	[Alias("Debtors"), Schema("findm")]

	public class Debtor
	{
		[AutoIncrement]
		public int Id { get; set; }

		[References(typeof(Person))]
		public int PersonId { get; set; }

		[References(typeof(DebtorType))]
		public int DebtorTypeId { get; set; }

		[References(typeof(BusinessPartner))]
		public int BusinessPartnerId { get; set; }

		public int Status { get; set; } 


	}
}