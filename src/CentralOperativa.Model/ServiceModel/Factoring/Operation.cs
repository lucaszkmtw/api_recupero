using System;
using System.Collections.Generic;
using CentralOperativa.Domain.Loans;
using CentralOperativa.Domain.System.DocumentManagement;
using CentralOperativa.ServiceModel.Financials;
using CentralOperativa.ServiceModel.System.DocumentManagement;
using CentralOperativa.ServiceModel.System.Messages;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Factoring
{
    [Route("/factoring/operationstatus", "GET")]
    public class GetOperationStatus
    {
    }

    [Route("/factoring/operations/{OperationGuid}/submitforauthorization", "POST")]
    public class PostSubmitOperationForAuthorization
    {
        public Guid OperationGuid { get; set; }
    }

    [Route("/loans/loans/{LoanGuid}/settlement", "POST")]
    public class PostLoanSettlementRequest
    {
        public Guid LoanGuid { get; set; }

        public List<GetLoanItemResult> Items { get; set; }
    }

    [Route("/factoring/operations/authorizations", "GET")]
    public class QueryOperationAuthorizations : QueryDb<Domain.Factoring.Operation, QueryOperationAuthorizationsResult>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }
    }

    public class QueryOperationAuthorizationsResult
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

    [Route("/factoring/operations", "GET")]
    public class QueryOperations : QueryDb<Domain.Factoring.Operation, QueryOperationResult>
    {
    }

    public class QueryOperationResult
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

    [Route("/factoring/operation/{Id}", "GET")]
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
        //public List<LoanInstallment.QueryLoanInstallmentResult> Installments { get; set; }
        public List<GetLoanPersonResult> Persons { get; set; }
        public DateTime? InstallmentFirstVoidDate { get; set; }
        public ServiceModel.System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse AuthorizationWorkflowInstance { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }
        public GetLoanResult()
        {
            this.Items = new List<GetLoanItemResult>();
            //this.Installments = new List<LoanInstallment.QueryLoanInstallmentResult>();
            this.Persons = new List<GetLoanPersonResult>();
        }
    }

    public class GetLoanItemResult : LoanItem
    {
        public LoanConcept Concept { get; set; }

        public List<GetLoanItemDistributionResult> Distributions { get; set; }

        public GetLoanItemResult()
        {
            this.Distributions = new List<GetLoanItemDistributionResult>();
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
            this.Persons = new List<PostLoanPerson>();
            this.Files = new List<File>();
            this.Items = new List<PostLoanItem>();
        }

        public class PostLoanPerson : Domain.Loans.LoanPerson
        {
            public ServiceModel.System.Persons.PostPerson Person { get; set; }
        }

        public class PostLoanItem : Domain.Loans.LoanItem
        {
            public PostLoanConcept Concept { get; set; }
        }

        public class PostLoanConcept : Domain.Loans.LoanConcept
        {
            public string BasedOnName { get; set; }
        }
    }
}