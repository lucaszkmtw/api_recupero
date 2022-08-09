using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentMethods")]
    public class PaymentMethod
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        public byte TypeId { get; set; }

        public string Name { get; set; }

        public bool AllowPayments { get; set; }

        public bool AllowReceipts { get; set; }
    }
}