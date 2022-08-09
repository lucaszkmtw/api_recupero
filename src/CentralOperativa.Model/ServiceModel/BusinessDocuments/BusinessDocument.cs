using System;
using System.Collections.Generic;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Persons;
using ServiceStack;

namespace CentralOperativa.ServiceModel.BusinessDocuments
{
    [Route("/businessdocuments/documents/{BusinessDocumentGuid}/submitforapproval", "POST")]
    public class PostBusinessDocumentSubmitForApproval
    {
        public Guid BusinessDocumentGuid { get; set; }
    }

    [Route("/businessdocuments/documents/approvals", "GET")]
    public class QueryBusinessDocumentApprovals : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentApprovalsResult>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }
        public BusinessDocumentModule Module { get; set; }
    }

    public class QueryBusinessDocumentApprovalsResult
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DocumentDate { get; set; }

        // BusinessDocumentType
        public string BusinessDocumentTypeName { get; set; }
        public string BusinessDocumentTypeShortName { get; set; }

        // Workflow
        public string WorkflowCode { get; set; }

        // Issuer
        public int IssuerId { get; set; }
        public string IssuerName { get; set; }

        //Receiver
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        // WorkflowActivity
        public int WorkflowActivityId { get; set; }
        public bool WorkflowActivityIsFinal { get; set; }
        public string WorkflowActivityName { get; set; }

        // WorkflowInstance
        public int WorkflowInstanceId { get; set; }
        public Guid WorkflowInstanceGuid { get; set; }
        public bool WorkflowInstanceIsTerminated { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }

        public string Roles { get; set; }
    }

    [Route("/businessdocuments/documents/{BusinessDocumentGuid}/submitforcollect", "POST")]
    public class PostBusinessDocumentSubmitForCollect
    {
        public Guid BusinessDocumentGuid { get; set; }
    }

    [Route("/businessdocuments/documents/collects", "GET")]
    public class QueryBusinessDocumentCollects : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentCollectsResult>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }
        public BusinessDocumentModule Module { get; set; }
    }

    public class QueryBusinessDocumentCollectsResult
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime VoidDate { get; set; }
        public decimal Total { get; set; }

        // BusinessDocumentType
        public string BusinessDocumentTypeName { get; set; }
        public string BusinessDocumentTypeShortName { get; set; }

        // Workflow
        public string WorkflowCode { get; set; }

        // Issuer
        public int IssuerId { get; set; }
        public string IssuerName { get; set; }

        //Receiver
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        // WorkflowActivity
        public int WorkflowActivityId { get; set; }
        public bool WorkflowActivityIsFinal { get; set; }
        public string WorkflowActivityName { get; set; }

        // WorkflowInstance
        public int WorkflowInstanceId { get; set; }
        public Guid WorkflowInstanceGuid { get; set; }
        public bool WorkflowInstanceIsTerminated { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }

        public string Roles { get; set; }
        public string DebtorName { get; set; }
    }

    [Route("/businessdocuments/documents/{BusinessDocumentGuid}/submitfordebtcollect", "POST")]
    public class PostBusinessDocumentSubmitForDebtCollect
    {
        public Guid BusinessDocumentGuid { get; set; }
    }
    [Route("/businessdocuments/documents/debtcollects", "GET")]
    public class QueryBusinessDocumentDebtCollects : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentCollectsResult>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }
        public int? Pending { get; set; }
        public BusinessDocumentModule Module { get; set; }
    }

    public class QueryBusinessDocumentDebtCollectsResult
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal Total { get; set; }

        // BusinessDocumentType
        public string BusinessDocumentTypeName { get; set; }
        public string BusinessDocumentTypeShortName { get; set; }

        // Workflow
        public string WorkflowCode { get; set; }

        // Issuer
        public int IssuerId { get; set; }
        public string IssuerName { get; set; }

        //Receiver
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        // WorkflowActivity
        public int WorkflowActivityId { get; set; }
        public bool WorkflowActivityIsFinal { get; set; }
        public string WorkflowActivityName { get; set; }

        // WorkflowInstance
        public int WorkflowInstanceId { get; set; }
        public Guid WorkflowInstanceGuid { get; set; }
        public bool WorkflowInstanceIsTerminated { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }

        public string Roles { get; set; }
    }

    [Route("/businessdocuments/documents/lookup", "GET")]
    public class LookupBusinessDocumentRequest : LookupRequest
    {
        public int? DebtorId { get; set; }
        public List<int> TypesId { get; set; }
    }

    [Route("/businessdocuments/documents", "GET")]
    public class QueryBusinessDocuments : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentsResult>
    {
        public string Q { get; set; }

        public BusinessDocumentModule Module { get; set; }

        public string TypeName { get; set; }

        public int? Status { get; set; }

        public int? RoleId { get; set; }
    }

    [Route("/businessdocuments/documentnads", "GET")]
    public class QueryBusinessDocumentNads : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentNadsResult>
    {
        public string Q { get; set; }

        public BusinessDocumentModule Module { get; set; }

        public string TypeName { get; set; }

        public int? Status { get; set; }
    }

    #region provisorio hasta terminar la tarea. Javier.enero 2017
    [Route("/businessdocuments/documents1/{Type}/{Module}", "GET")]
    public class QueryBusinessDocument1 : QueryDb<Domain.BusinessDocuments.BusinessDocument, QueryBusinessDocumentResult1>
    {
        public string Type { get; set; }

        public BusinessDocumentModule Module { get; set; }
    }

    public class QueryBusinessDocumentResult1
    {
        public int Id { get; set; }

        public string TypeName { get; set; }

        public string TypeShortName { get; set; }

        public string TypeCode { get; set; }

        public DateTime DocumentDate { get; set; }

        public string Number { get; set; }

        public byte Status { get; set; }

        public byte FinancingStatus { get; set; }

        public string IssuerName { get; set; }

        public string ReceiverName { get; set; }

        public decimal Total { get; set; }

        public int? ApprovalWorkflowInstanceId { get; set; }

        public string Roles { get; set; }
    }
    #endregion

    public enum BusinessDocumentModule
    {
        All,
        AccountsPayables,
        AccountsReceivables,
        Inventory = 6,
        Collector = 7,
        DebtCollector = 8
    }

    public class QueryBusinessDocumentsResult
    {
        public int Id { get; set; }

        public string TypeName { get; set; }

        public string TypeCode { get; set; }

        public DateTime DocumentDate { get; set; }

        public string Number { get; set; }

        public byte Status { get; set; }

        public byte FinancingStatus { get; set; }

        public string IssuerName { get; set; }

        public string ReceiverName { get; set; }

        public string CategoryName { get; set; }

        public decimal Total { get; set; }

        public int? ApprovalWorkflowInstanceId { get; set; }

        public string Roles { get; set; }

        public string WorkflowActivityName { get; set; }
    }

    public class QueryBusinessDocumentNadsResult
    {
        public int Id { get; set; }

        //public string TypeName { get; set; }

        //public string TypeCode { get; set; }

        public DateTime DocumentDate { get; set; }

        public string Number { get; set; }

        public byte Status { get; set; }

        public byte FinancingStatus { get; set; }

        //public string IssuerName { get; set; }

        public string ReceiverName { get; set; }

        //public string CategoryName { get; set; }

        public decimal Total { get; set; }

        public int? ApprovalWorkflowInstanceId { get; set; }

        public string Roles { get; set; }
    }


    [Route("/businessdocuments/documents/{Id}", "GET")]
    public class GetBusinessDocument
    {
        public int Id { get; set; }

        public bool Edit { get; set; }
    }

    public class BusinessDocument : Domain.BusinessDocuments.BusinessDocument
    {
        public BusinessDocument()
        {
            Items = new List<Domain.BusinessDocuments.BusinessDocumentItem>();
        }

        public List<Domain.BusinessDocuments.BusinessDocumentItem> Items { get; set; }

        public WorkflowInstance ApprovalWorkflowInstance { get; set; }

        public Person Issuer { get; set; }

        public Person Receiver { get; set; }

        public Person Dispatcher { get; set; }

        public Domain.Inv.InventorySite Site { get; set; }
    }


    public class BusinessDocumentItem : Domain.BusinessDocuments.BusinessDocumentItem
    {
        public Product Product { get; set; }
        public Domain.Inv.InventorySite Site { get; set; }
    }

    [Route("/businessdocuments/documents", "POST")]
    [Route("/businessdocuments/documents/{Id}", "POST, PUT")]
    public class PostBusinessDocument : Domain.BusinessDocuments.BusinessDocument
    {
        public PostBusinessDocument()
        {
            Items = new List<Domain.BusinessDocuments.BusinessDocumentItem>();
        }

        public List<Domain.BusinessDocuments.BusinessDocumentItem> Items { get; set; }
    }

    [Route("/businessdocuments/batch", "POST")]
    public class PostBusinessDocumentBatch : List<PostBusinessDocumentBatchItem>
    {
    }

    public class PostBusinessDocumentBatchItem
    {
        public Domain.System.ImportLog ImportLog { get; set; }
        public PostBusinessDocument BusinessDocument { get; set; }
    }

    [Route("/businessdocuments/documents/{Id}", "DELETE")]
    public class DeleteBusinessDocument : IReturnVoid
    {
        public int Id { get; set; }
    }


    [Route("/businessdocuments/documents/{TypeId}/results", "GET")]
    public class GetBusinessDocumentResults : IReturn<ExcelFileResult>
    {
        public int TypeId { get; set; }
        public string Exporter { get; set; }
    }

    [Route("/businessdocuments/documents/collection", "POST")]
    [Route("/businessdocuments/documents/collection/{Id}", "PUT")]
    public class PostBusinessDocumentCollection : Domain.BusinessDocuments.BusinessDocument
    {
        public PostBusinessDocumentCollection()
        {
            Items = new List<BusinessDocumentItemDetail>();
            Messages = new List<ServiceModel.System.Messages.Message.QueryResult>();
            Reckonings = new List<BusinessDocumentHeader>();
            PaymentCoupons = new List<BusinessDocumentHeader>();
            ExecutionDocuments = new List<BusinessDocumentHeader>();
            ParentItems = new List<ParentItem>();
        }
        public List<BusinessDocumentItemDetail> Items { get; set; }
        public WorkflowInstance CollectWorkflowInstance { get; set; }
        public List<ServiceModel.System.Messages.Message.QueryResult> Messages { get; set; }
        public bool EditPermissions { get; set; }
        public List<BusinessDocumentHeader> Reckonings { get; set; }
        public List<BusinessDocumentHeader> PaymentCoupons { get; set; }
        public List<BusinessDocumentHeader> ExecutionDocuments { get; set; }
        public List<ParentItem> ParentItems { get; set; }
        public string TypeName { get; set; }
        public string Normatives { get; set; }
        public string IssuerName { get; set; }
        public string IssuerCode { get; set; }
        public string DebtorName { get; set; }
        public string DebtorCode { get; set; }
        public string DebtorAddress { get; set; }
        public string DebtorAddressTwo { get; set; }
        public string DebtorRNOS { get; set; }
        public string TotalText { get; set; }
        public string CreditorBankAccountCode { get; set; }
        public string CreditorBankAccountNumber { get; set; }
        public string CreditorBankAccountDescription { get; set; }
        public string CreditorBankAccountBranch { get; set; }
        public string CreditorName { get; set; }
        public string CreditorCode { get; set; }
        
    }

    [Route("/businessdocuments/documents/collection/{Id}/paymentcoupon", "POST")]
    public class PostBusinessDocumentPaymentCoupon
    {
        public int Id { get; set; }
    }

    [Route("/businessdocuments/documents/collection/{Id}/execution", "POST")]
    public class PostBusinessDocumentExecution
    {
        public int Id { get; set; }
    }

    [Route("/businessdocuments/documents/reckoning", "POST")]
    public class PostBusinessDocumentReckoning : PostBusinessDocumentCollection
    {
        public PostBusinessDocumentReckoning()
        {
            ParentItems = new List<ParentItem>();
        }
        public int? BusinessDocumentParentId { get; set; }
        public List<ParentItem> ParentItems { get; set; }
        public decimal InterestTotal { get; set; }
    }

    public class ParentItem
    {
        public ParentItem()
        {
            Applications = new List<BusinessDocumentItemApplication>();
        }

        public int ParentId { get; set; } //bdi que genera estos montos
        public double Amount { get; set; } //Monto generado
        public double AmountInterest { get; set; } //Monto generado
        public double AppliedAmount { get; set; } //Monto generado
        public DateTime FromDate { get; set; } //Parametro de calculo de intereses
        public DateTime ToDate { get; set; } //Parametro de calculo de intereses  
        public int DocumentItemRelatedId { get; set; } //para uso de api unicamente
        public string ProductName { get; set; } //Dato para el front
        public double? InterestApplication { get; set; } 
        public double? CapitalApplication { get; set; }
        public double? PendingInterest { get; set; }
        public List<BusinessDocumentItemApplication> Applications { get; set; }
    }

    public class BusinessDocumentItemDetail : BusinessDocumentItem
    {   
        public BusinessDocumentItemDetail()
        {
            Creditors = new List<int>();
            Debtors = new List<int>();
            LawTexts = new List<BusinessDocumentItemDetailaw>();          

        }
        public List<int> Creditors { get; set; }
        public List<int> Debtors { get; set; }
        public List<BusinessDocumentItemDetailaw> LawTexts { get; set; }
        
        public int Status { get; set; }

    }
    public class BusinessDocumentItemApplication
    {
        public double Amount { get; set; }
        public string ApplicationDocumentNumber { get; set; }
        public int ApplicationDocumentId { get; set; }
        public DateTime ApplicationDocumentCreateDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double AppliedAmount { get; set; }
    }

    public class BusinessDocumentHeader
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
        public decimal Total { get; set; }
        public decimal Interest { get; set; }
        public decimal Base { get; set; }
        public string LinkedNumber { get; set; }
        public DateTime? VoidDate { get; set; }
        public string TypeName { get; set; }
        public string CurrentWorkflowActivityName { get; set; }
        public DateTime? FromServiceDate { get; set; }
        public DateTime? ToServiceDate { get; set; }
    }

    public class BusinessDocumentItemDetailaw
    {
        public int LawId { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public int Prescription { get; set; }
    }

    [Route("/businessdocuments/documents/collection/{Id}", "GET")]
    public class GetBusinessDocumentCollect
    {
        public int Id { get; set; }

        public bool Edit { get; set; }
    }

    [Route("/businessdocuments/documents/collectionexecute/{Id}", "GET")]
    public class GetBusinessDocumentCollectExecute
    {
        public int Id { get; set; }

        public bool Edit { get; set; }
    }

    public class BusinessDocumentLinkResults
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int LinkedId { get; set; }
    }

    [Route("/businessdocuments/documents/getpdf/{Id}", "GET")]
    public class GetBusinessDocumentGetPDF
    {
        public int Id { get; set; }
    }

    [Route("/businessdocuments/documents/gettepdf/{Id}", "GET")]
    public class GetBusinessDocumentGetTePDF
    {
        public int Id { get; set; }
    }
    [Route("/businessdocuments/documents/getteattachpdf/{Id}", "GET")]
    public class GetBusinessDocumentGetTeAttachPDF
    {
        public int Id { get; set; }
    }

    [Route("/businessdocuments/files", "POST")]
    public class PostBusinessDocumentFile : IReturn<PostBusinessDocumentFileResult>
    {
        public string FileName { get; set; }
    }
    public class PostBusinessDocumentFileResult : PostBusinessDocumentFile
    {
    }

    [Route("/businessdocuments/import", "POST")]
    public class ImportBusinessDocuments : IReturn<ImportBusinessDocumentsResult>
    {
        public string FileName { get; set; }
        public List<string> Columns { get; set; }
        public int PaymentMethodId { get; set; }
        public bool PayExistingDebt { get; set; }
        public int DebtorId { get; set; }
        public int CreditorId { get; set; }
        public string BusinessDocumentNumber { get; set; }
    }
    public class ImportBusinessDocumentsResult : ImportBusinessDocuments
    {
        public int InsertedItemsCount { get; set; }
    }
}