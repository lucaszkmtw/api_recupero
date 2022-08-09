using System;
using ServiceStack.DataAnnotations;
namespace CentralOperativa.Domain.CRM
{
    [Alias("CampaignProducts")]
    public class CampaignProduct
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CampaignId { get; set; }
    }
}
