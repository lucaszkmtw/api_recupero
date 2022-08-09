using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentAgreements")]
    public class PaymentAgreement
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Tenant))]
        public int TenantId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }


    }
}
