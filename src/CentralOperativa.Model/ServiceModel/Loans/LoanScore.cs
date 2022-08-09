using System;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Loans
{
    [Route("/loans/scoring", "POST")]
    public class PostCreditScoreRequest : BancoDeComercioScoringRequest
    {
        public int LoanLenderId { get; set; }

        public int PersonId { get; set; }

        public int? LoanId { get; set; }

        public int Term { get; set; }
    }

    [Route("/loans/loans/{LoanGuid}/scoring", "GET")]
    public class QueryLoanScores : IReturn<QueryResponse<QueryCreditScoresResult>>
    {
        public Guid LoanGuid { get; set; }
    }

    [Route("/system/persons/{PersonId}/scoring", "GET")]
    public class QueryPersonScores : IReturn<QueryResponse<QueryCreditScoresResult>>
    {
        public int PersonId { get; set; }
    }

    public class QueryCreditScoresResult
    {
        public int Id { get; set; }

        public int LoanLenderId { get; set; }

        public string LoanLenderName { get; set; }

        public byte? Status { get; set; }

        public decimal? Score { get; set; }

        public decimal? RCI { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public decimal? Installment { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string Comments { get; set; }

        public string Result { get; set; }

        public bool Accepted { get; set; }
    }
}