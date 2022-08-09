using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentItemCreditors")]
    public class BusinessDocumentItemCreditor
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessDocumentItem))]
        public int BusinessDocumentItemId { get; set; }

        [References(typeof(Domain.Financials.DebtManagement.Creditor))]
        public int CreditorId { get; set; }

        public decimal Amount { get; set; }

    }
}