using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms
{
    [Alias("Sections")]
    public class Section
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Section))]
        public int? ParentId { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        public int TemplateId { get; set; }

        public string Name { get; set; }

        public string SeName { get; set; }
        
        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public string Link { get; set; }

        public byte LinkTarget { get; set; }

        public bool AllowAnonymous { get; set; }

        public byte OrderItemsBy { get; set; }

        public byte OrderBy { get; set; }

        public short ListIndex { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}