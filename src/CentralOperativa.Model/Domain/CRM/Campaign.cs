using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.CRM
{
    [Alias("Campaigns")]
    public class Campaign
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public int? FormId { get; set; }
    }
}
