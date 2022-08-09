using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentItemDebtors")]
    public class BusinessDocumentItemDebtor

    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessDocumentItem))]
        public int BusinessDocumentItemId { get; set; }

        [References(typeof(Domain.Financials.DebtManagement.Debtor))]
        public int DebtorId { get; set; }

        public decimal Amount { get; set; }

    }
}