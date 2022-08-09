using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Financials;
using CentralOperativa.Domain.Investments;

namespace CentralOperativa.Domain.Factoring
{
    [Alias("OperationPaymentDocuments"), Schema("factoring")]
    public class OperationPaymentDocument
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Operation))]
        public int OperationId{ get; set; }

        [References(typeof(PaymentDocument))]
        public int PaymentDocumentId { get; set; }

        [References(typeof(Investor))]
        public int? InvestorId { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Capacity { get; set; }

        public decimal Comission { get; set; }

        public decimal Expenses { get; set; }

        public short Term { get; set; }
    }
}

