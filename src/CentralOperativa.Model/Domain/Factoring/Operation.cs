using System;

using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Domain.System.Messages;
using CentralOperativa.Domain.System.DocumentManagement;

namespace CentralOperativa.Domain.Factoring
{
    [Alias("Operations"), Schema("factoring")]
    public class Operation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId{ get; set; }

        [References(typeof(Client))]
        public int ClientId { get; set; }

        [Alias("SourceId")]
        public OperationSource Source { get; set; }

        public byte TypeId { get; set; }

        [References(typeof(MessageThread))]
        public int? MessageThreadId { get; set; }

        [References(typeof(Folder))]
        public int? FolderId { get; set; }

        [References(typeof(WorkflowInstance))]
        public int? AuthorizationWorkflowInstanceId { get; set; }

        public Guid Guid { get; set; }

        public string Number { get; set; }

        public decimal Amount { get; set; }

        public decimal Expenses { get; set; }

        public decimal Due { get; set; }

        public decimal Balance { get; set; }

        public int Term { get; set; }

        [Alias("StatusId")]
        public OperationStatus Status { get; set; }

        public DateTime Date { get; set; }
    }

    public enum OperationSource
    {
        PaymentDocument,
        BusinessDocument
    }
}

