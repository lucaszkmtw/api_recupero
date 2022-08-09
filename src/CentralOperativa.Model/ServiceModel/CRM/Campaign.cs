using ServiceStack;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.CRM
{
    [Route("/crm/campaigns/", "GET")]
    public class QueryCampaigns : QueryDb<Domain.CRM.Campaign, QueryCampaignsResult>
    {

    }

    [Route("/crm/campaign/{Id}", "GET")]
    public class GetCampaign : Domain.CRM.Campaign
    {
        public List<Form> Forms { get; set; }

        public class Form
        {
            public int Id { get; set; }
            public int FormId { get; set; }
            public string FormName { get; set; }
        }

        public List<Product> Products { get; set; }

        public class Product
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
        }
    }

    [Route("/crm/campaign", "POST")]
    [Route("/crm/campaign/{Id}", "PUT")]
    public class PostCampaign : Domain.CRM.Campaign
    {
        public List<Form> Forms { get; set; }

        public class Form
        {
            public int? Id { get; set; }

            public int FormId { get; set; }

        }
        public List<Product> Products { get; set; }

        public class Product
        {
            public int? Id { get; set; }

            public int ProductId { get; set; }

        }
    }

    [Route("/crm/campaign/lookup", "GET")]
    public class LookupCampaignsRequest : LookupRequest, IReturn<List<LookupItem>>
    {

    }

    public class QueryCampaignsResult
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormName { get; set; }
    }
}