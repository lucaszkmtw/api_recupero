using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("OrganismTypes"),Schema("findm")]
	
	public class OrganismType
    {
        [AutoIncrement]
        public int Id { get; set; }
		public string Code { get; set; }
        public string Name { get; set; }

        [References(typeof(BusinessPartnerType))]
        public int BusinessPartnerTypeId { get; set; }

        public int Status { get; set; }
    }
}