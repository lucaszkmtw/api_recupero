using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocuments")]
    public class BusinessDocument
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.BusinessDocuments.BusinessDocumentType))]
        public short TypeId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int IssuerId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int ReceiverId { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedBy { get; set; }

        [References(typeof(Domain.System.DocumentManagement.Folder))]
        public int? FolderId { get; set; }

        [References(typeof(Domain.System.Messages.MessageThread))]
        public int? MessageThreadId { get; set; }

        public byte ItemTypesId { get; set; }

        public string Comments { get; set; }

        public Guid Guid { get; set; }

        public BusinessDocumentStatus Status { get; set; }

        public int? ApprovalWorkflowInstanceId { get; set; }

        public string Number { get; set; }
        
        public DateTime CreateDate { get; set; }

        public DateTime DocumentDate { get; set; }

        public DateTime? FromServiceDate { get; set; }

        public DateTime? ToServiceDate { get; set; }

        public decimal Total { get; set; }

        public string CAE { get; set; }

        public DateTime? CAEVoidDate { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int? DispatcherId { get; set; }

        public int? InventorySiteId { get; set; }

        public DateTime? VoidDate { get; set; }

        public DateTime? NotificationDate { get; set; }

        [References(typeof(Domain.Catalog.Category))]
        public int? CategoryId { get; set; }
        
    }
}