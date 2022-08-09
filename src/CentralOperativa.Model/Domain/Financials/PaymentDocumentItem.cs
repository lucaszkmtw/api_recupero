using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocumentItems")]
    public class PaymentDocumentItem
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(PaymentDocument))]
        public int PaymentDocumentId { get; set; }

        [References(typeof(EntityType))]
        public short LinkedDocumentTypeId { get; set; }

        public int LinkedDocumentId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public decimal OriginalAmount { get; set; }

        public decimal AmountToPay { get; set; }
    }
}