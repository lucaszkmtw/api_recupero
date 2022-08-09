using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.CRM
{
    [Alias("Leads")]
    public class Lead
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string Source { get; set; }
        public int Status { get; set; }
        public int FormResponseId { get; set; }
        public int? AuthorizationWorkflowInstanceId { get; set; }
        public int? MessageThreadId { get; set; }
        public int? FolderId { get; set; }
        public int? CampaignId { get; set; }
    }
}
