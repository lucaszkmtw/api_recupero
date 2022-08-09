using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials.Controlling
{
    [Alias("CostCenters"), Schema("financials")]
    public class CostCenter
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Currency))]
        public short CurrenctyId { get; set; }

        public string Name { get; set; }
    }
}
