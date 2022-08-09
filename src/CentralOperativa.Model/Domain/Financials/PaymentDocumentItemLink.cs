using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocumentItemLinks")]
    public class PaymentDocumentItemLink
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(PaymentDocumentLink))]
        public int PaymentDocumentLinkId { get; set; }

        [References(typeof(BusinessDocuments.BusinessDocumentItem))]
        public int BusinessDocumentItemId { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedBy { get; set; }

        public decimal ApplicationAmount { get; set; }
        public DateTime CreateDate { get; set; }
    }
}