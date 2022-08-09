using System.Collections.Generic;
using CentralOperativa.Domain.Loans;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Loans
{
    [Route("/loans/concepts", "GET")]
    public class QueryLoanConcepts : QueryDb<LoanConcept, QueryLoanConceptsResult>
    {
    }

    public class QueryLoanConceptsResult : LoanConcept
    {
    }

    [Route("/loans/concepts/{Id}", "GET")]
    public class GetLoanConcept : IReturn<GetLoanConceptResult>
    {
        public int Id { get; set; }
    }

    public class GetLoanConceptResult : LoanConcept
    {
        public List<LoanConceptDistribution> Applications { get; set; }

        public GetLoanConceptResult()
        {
            this.Applications = new List<LoanConceptDistribution>();
        }
    }

    [Route("/loans/concepts", "POST")]
    [Route("/loans/concepts/{id}", "PUT")]
    public class PostLoanConcept : LoanConcept
    {
    }
}