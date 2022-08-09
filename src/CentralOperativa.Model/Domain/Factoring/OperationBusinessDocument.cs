using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.BusinessDocuments;
using CentralOperativa.Domain.Investments;

namespace CentralOperativa.Domain.Factoring
{
    [Alias("OperationBusinessDocuments"), Schema("factoring")]
    public class OperationBusinessDocument
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Operation))]
        public int OperationId{ get; set; }

        [References(typeof(BusinessDocument))]
        public int BusinessDocumentId { get; set; }

        [References(typeof(Investor))]
        public int InvestorId { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Capacity { get; set; }

        public decimal Comission { get; set; }

        public decimal Expenses { get; set; }
    }
}

