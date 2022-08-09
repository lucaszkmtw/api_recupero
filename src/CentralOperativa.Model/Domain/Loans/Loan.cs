using System;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Workflows;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("Loans")]
    public class Loan
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId{ get; set; }

        public Guid Guid { get; set; }

        public string Number { get; set; }

        [References(typeof(WorkflowInstance))]
        public int? AuthorizationWorkflowInstanceId { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        public decimal Amount { get; set; }

        public int Term { get; set; }

        [Alias("StatusId")]
        public LoanStatus Status { get; set; }

        public DateTime Date { get; set; }

        public int? MessageThreadId { get; set; }

        public int? FolderId { get; set; }

        public decimal InstallmentBaseAmount { get; set; }
        public decimal Due { get; set; }
        public decimal Balance { get; set; }
        public decimal Expenses { get; set; }
    }
}

