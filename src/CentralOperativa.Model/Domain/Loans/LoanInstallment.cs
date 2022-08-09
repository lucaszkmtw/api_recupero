using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanInstallments")]
    public class LoanInstallment
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public int StateId { get; set; }
        public DateTime VoidDate { get; set; }
        public short Number { get; set; }
        public decimal Capital { get; set; }
        public decimal Interests { get; set; }
        public decimal Taxes { get; set; }
        public decimal Balance { get; set; }
    }
}
