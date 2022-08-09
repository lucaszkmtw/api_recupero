using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms.Forms
{
    [Alias("Forms")]
    public class Form
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Remarks { get; set; }

        public string FinalMessage { get; set; }

        public int MinQuota { get; set; }

        public int Quota { get; set; }

        public bool AllowDrafts { get; set; }

        public bool AllowQueue { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public DateTime? Date { get; set; }

        public bool ValidateUser { get; set; }

        public bool ShowUserData { get; set; }

        public bool ShowReceipt { get; set; }

        public string ReceiptFooter { get; set; }

        public bool AllowUpdates { get; set; }

        public Guid Guid { get; set; }

        public bool ShowTitle { get; set; }

        public string HeaderImage { get; set; }

        public string Place { get; set; }

        public string Contact { get; set; }

        public string Configuration { get; set; }

        public dynamic Fields { get; set; }

        public bool IsPublished { get; set; }

        public byte TypeId { get; set; }
        public string Template { get; set; }
    }
}