using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanScores")]
    public class LoanScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(System.Persons.Person))]
        public int PersonId { get; set; }

        [References(typeof(Loan))]
        public int? LoanId { get; set; }

        [References(typeof(LoanLender))]
        public int LoanLenderId { get; set; }

        public DateTime CreateDate { get; set; }

        public string Data { get; set; }

        public bool Accepted { get; set; }

        public string Result { get; set; }

        public byte? Status { get; set; }

        public decimal? Score { get; set; }

        public decimal? RCI { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public decimal? Installment { get; set; }

        public DateTime? DueDate { get; set; }

        public string Comments { get; set; }
    }
}
