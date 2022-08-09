using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocumentLinks")]
    public class PaymentDocumentLink
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int PaymentDocumentId { get; set; }

        [References(typeof(EntityType))]
        public short LinkedDocumentTypeId { get; set; }

        public int LinkedDocumentId { get; set; }
    }
}