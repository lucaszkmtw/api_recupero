using System;
using System.Collections.Generic;

using ServiceStack;

using CentralOperativa.Domain.Loans;
using CentralOperativa.Domain.System.DocumentManagement;
using CentralOperativa.ServiceModel.Financials;
using CentralOperativa.ServiceModel.System.DocumentManagement;
using CentralOperativa.ServiceModel.System.Messages;

namespace CentralOperativa.ServiceModel.Loans
{
    [Route("/loans/loanstatus", "GET")]
    public class GetLoanStatus
    {
    }

    [Route("/loans/loans/{LoanGuid}/submitforauthorization", "POST")]
    public class PostSubmitForAuthorizationRequest
    {
        public Guid LoanGuid { get; set; }
    }

    [Route("/loans/loans/{LoanGuid}/settlement", "POST")]
    public class PostLoanSettlementRequest
    {
        public Guid LoanGuid { get; set; }

        public List<GetLoanItemResult> Items { get; set; }
    }

    [Route("/loans/loans/authorizations", "GET")]
    public class QueryLoansAuhtorizations : QueryDb<Loan, QueryLoansAuthorizationsResult>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }
    }

    public class QueryLoansAuthorizationsResult
    {
        public int Id { get; set; }
        public string Number { get; set; }

        public Guid Guid { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        // Workflow
        public string WorkflowCode { get; set; }

        // Person
        public int PersonId { get; set; }
        public string PersonName { get; set; }

        // WorkflowActivity
        public int WorkflowActivityId { get; set; }
        public bool WorkflowActivityIsFinal { get; set; }
        public string WorkflowActivityName { get; set; }

        // WorkflowInstance
        public int WorkflowInstanceId { get; set; }
        public Guid WorkflowInstanceGuid { get; set; }
        public bool WorkflowInstanceIsTerminated { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }
        // WorkflowInstanceApproval
        public int? WorkflowInstanceApprovalId { get; set; }
        public DateTime? WorkflowInstanceApprovalCreateDate { get; set; }

        public string Roles { get; set; }
    }

    [Route("/loans/loans", "GET")]
    public class QueryLoans : QueryDb<Loan, QueryLoansResult>
    {
    }

    public class QueryLoansResult
    {
        public int Id { get; set; }
        public string SellerName { get; set; }
        public string ApplicantName { get; set; }
        public string ProductName { get; set; }
        public int TenantId { get; set; }
        public int ProductCatalogId { get; set; }
        public int? AuthorizationWorkflowInstanceId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public decimal Amount { get; set; }
        public int Term { get; set; }
        public byte Status { get; set; }
        public DateTime? Date { get; set; }
        public decimal InstallmentBaseAmount { get; set; }

        public List<string> Roles { get; set; }
    }

    [Route("/loans/loans/{Id}", "GET")]
    public class GetLoan : IReturn<GetLoanResult>
    {
        public int Id { get; set; }
    }

    public class GetLoanResult : Loan
    {
        public string ProductName { get; set; }
        public string Name { get; set; }
        public List<string> Roles { get; set; }
        public decimal InstallmentAmount { get; set; }
        public List<GetLoanItemResult> Items { get; set; }
        public List<LoanInstallment.QueryLoanInstallmentResult> Installments { get; set; }
        public List<GetLoanPersonResult> Persons { get; set; }
        public DateTime? InstallmentFirstVoidDate { get; set; }
        public System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse AuthorizationWorkflowInstance { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }
        public GetLoanResult()
        {
            Items = new List<GetLoanItemResult>();
            Installments = new List<LoanInstallment.QueryLoanInstallmentResult>();
            Persons = new List<GetLoanPersonResult>();
        }
    }

    public class GetLoanItemResult : LoanItem
    {
        public LoanConcept Concept { get; set; }

        public List<GetLoanItemDistributionResult> Distributions { get; set; }

        public GetLoanItemResult()
        {
            Distributions = new List<GetLoanItemDistributionResult>();
        }

        public class GetLoanItemDistributionResult : LoanItemDistribution
        {
            public GetPaymentDocumentItemResponse PaymentDocumentItem { get; set; }
        }
    }

    public class GetLoanPersonResult : LoanPerson
    {
        public System.Persons.Person Person { get; set; }
    }

    [Route("/loans/loans/batch", "POST")]
    public class PostLoanBatch : List<PostLoanBatchItem>
    {
    }
    
    [Route("/loans/loans/{loanId}", "PUT")]
    public class PutLoan
    {
        public int LoanId { get; set; }
        public DateTime VoidDate { get; set; }
    }


    public class PostLoanBatchItem
    {
        public Domain.System.ImportLog ImportLog { get; set; }
        public PostLoan Loan { get; set; }
    }

    public class PostLoan : Loan
    {
        public Catalog.PostProduct Product { get; set; }

        public List<PostLoanItem> Items { get; set; }
        public List<PostLoanPerson> Persons { get; set; }

        public List<File> Files { get; set; }
        public DateTime InitialVoidDate { get; set; }

        public PostLoan()
        {
            Persons = new List<PostLoanPerson>();
            Files = new List<File>();
            Items = new List<PostLoanItem>();
        }

        public class PostLoanPerson : LoanPerson
        {
            public System.Persons.PostPerson Person { get; set; }
        }

        public class PostLoanItem : LoanItem
        {
            public PostLoanConcept Concept { get; set; }
        }

        public class PostLoanConcept : LoanConcept
        {
            public string BasedOnName { get; set; }
        }
    }
}