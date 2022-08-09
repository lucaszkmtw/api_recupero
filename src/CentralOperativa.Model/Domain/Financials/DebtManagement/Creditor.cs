using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.Domain.Financials.DebtManagement
{

	[Alias("Creditors"), Schema("findm")]

	public class Creditor
	{
		[AutoIncrement]
		public int Id { get; set; }

		[References(typeof(Person))]
		public int PersonId { get; set; }
				
		[References(typeof(BusinessPartner))]
		public int BusinessPartnerId { get; set; }

		public int Status { get; set; } 
		
	}
}