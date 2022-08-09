using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("BankAccountEntries")]
    public class BankAccountEntry
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.Financials.BankAccount))]
        public int BankAccountId { get; set; }

        public decimal Amount { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime PostingDate { get; set; }

        public short? LinkedDocumentTypeId { get; set; }

        public int? LinkedDocumentId { get; set; }
    }
}