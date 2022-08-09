using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("CreditTypes"),Schema("findm")]
	
	public class CreditType
    {
        [AutoIncrement]
        public int Id { get; set; }
		public string Code { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
    }
}