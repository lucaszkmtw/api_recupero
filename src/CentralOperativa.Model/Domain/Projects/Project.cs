using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("Projects"), Schema("projects")]
    public class Project
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedBy { get; set; }

        [References(typeof(Domain.System.DocumentManagement.Folder))]
        public int? FolderId { get; set; }

        [References(typeof(Domain.System.Messages.MessageThread))]
        public int? MessageThreadId { get; set; }

        [References(typeof(Domain.Projects.FundingType))]
        public int? FundingTypeId { get; set; }

        [References(typeof(Domain.System.Workflows.WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        public byte CurrencyId { get; set; }

        public string Number { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Review { get; set; }

        public Guid Guid { get; set; }

        public ProjectStatus Status { get; set; }
        
        public DateTime CreateDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public decimal? Investment { get; set; }

        public decimal? ContractAmount { get; set; }

        public decimal? AditionalAmount { get; set; }

        public decimal? AdjustedAmount { get; set; }

        public decimal? Total { get; set; }
    }
}