using System;
using System.Collections.Generic;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Loans
{
    public class LoanInstallment
    {
        [Route("/loans/loans/{loanId}/installments", "GET")]
        public class GetLoanInstallments : QueryDb<Domain.Loans.LoanInstallment, QueryLoanInstallmentResult>
        {
            public int LoanId { get; set; }
        }

        [Route("/loans/loans/{loanId}/installments", "POST")]
        public class PostLoanInstallments : QueryDb<Domain.Loans.LoanInstallment, QueryLoanInstallmentResult>
        {
            public int LoanId { get; set; }
            public decimal Amount { get; set; }
            public int Term { get; set; }
            public decimal InstallmentBaseAmount { get; set; }
            public DateTime? Date { get; set; }
        }
        
        public class QueryLoanInstallmentResult
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public decimal Amount { get; set; }
            public int StateId { get; set; }
            public DateTime? VoidDate { get; set; }
            public decimal Capital { get; set; }
            public decimal Interests { get; set; }
            public decimal Taxes { get; set; }
            public decimal Balance { get; set; }
            //public decimal SocialInstallment { get; set; }
            //public decimal Due { get; set; }
        }
    }
}
