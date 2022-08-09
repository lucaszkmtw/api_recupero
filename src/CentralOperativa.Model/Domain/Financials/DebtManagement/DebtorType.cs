using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("DebtorTypes"),Schema("findm")]
	
	public class DebtorType
    {
        [AutoIncrement]
        public int Id { get; set; }

		public int BusinessPartnerTypeId { get; set; }

        public string Name { get; set; }

		public int Status { get; set; }
	}
}