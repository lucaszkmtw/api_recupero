using System;
using ServiceStack.DataAnnotations;
namespace CentralOperativa.Domain.CRM
{
    [Alias("CampaignForms")]
    public class CampaignForm
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int FormId { get; set; }
        public int CampaignId { get; set; }
    }
}
