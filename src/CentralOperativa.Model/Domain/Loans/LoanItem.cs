using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanItems")]
    public class LoanItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Loan))]
        public int LoanId{ get; set; }

        [References(typeof(LoanConcept))]
        public int ConceptId { get; set; }

        public decimal Value { get; set; }
    }
}

