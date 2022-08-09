using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("InvestmentAccountEntries")]
    public class InvestmentAccountEntry
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.Financials.InvestmentAccount))]
        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime PostingDate { get; set; }

        public short? LinkedDocumentTypeId { get; set; }

        public int? LinkedDocumentId { get; set; }
    }
}
