using System;
using System.Linq;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.CRM;
using CentralOperativa.Infraestructure;
using System.Collections.Generic;

namespace CentralOperativa.ServiceInterface.CRM.Contacts
{
    [Authenticate]
    public class CampaignService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(CampaignService));

        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.QueryCampaigns request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<Domain.CRM.Campaign>(c => c.TenantId == this.Session.TenantId);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.GetCampaign request)
        {
            var campaign = Db.Select(Db.From<Domain.CRM.Campaign>().Where(c => c.Id == request.Id))
                .SingleOrDefault()
                .ConvertTo<Api.GetCampaign>();

            var forms = Db.Select<Api.GetCampaign.Form>(
                    Db.From<Domain.CRM.CampaignForm>()
                    .Join<Domain.CRM.CampaignForm, Domain.Cms.Forms.Form>()
                    .Where<Domain.CRM.CampaignForm>(x => x.CampaignId == request.Id));
            campaign.Forms = forms;

            var products = Db.Select<Api.GetCampaign.Product>(
                    Db.From<Domain.CRM.CampaignProduct>()
                    .Join<Domain.CRM.CampaignProduct, Domain.Catalog.Product>()
                    .Where<Domain.CRM.CampaignProduct>(x => x.CampaignId == request.Id));
            campaign.Products = products;
            return campaign;
        }

        public Api.PostCampaign Post(Api.PostCampaign request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var campaign = new Domain.CRM.Campaign
                    {
                        TenantId = this.Session.TenantId,
                        Name = request.Name,
                        Description = request.Description
                    };
                    request.Id = (int)Db.Insert(campaign, true);
                    Save(request);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public Api.PostCampaign Put(Api.PostCampaign request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((Domain.CRM.Campaign)request);
                    Save(request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;
        }
        
        private void Save(Api.PostCampaign request)
        {

            var campaignformIds = request.Forms.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (campaignformIds.Any())
            {
                Db.Delete<Domain.CRM.CampaignForm>(x => x.CampaignId == request.Id && !Sql.In(x.Id, campaignformIds));
            }
            else
            {
                Db.Delete<Domain.CRM.CampaignForm>(x => x.CampaignId == request.Id);
            }

            foreach (var campaignForm in request.Forms)
            {
                if (campaignForm.Id.HasValue)
                {
                    Db.Update(new Domain.CRM.CampaignForm
                    {
                        Id = campaignForm.Id.Value,
                        CampaignId = request.Id,
                        FormId = campaignForm.FormId
                    });
                }
                else
                {
                    Db.Insert(new Domain.CRM.CampaignForm
                    {
                        CampaignId = request.Id,
                        FormId = campaignForm.FormId
                    });
                }
            }
            
            var campaignproductIds = request.Products.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (campaignproductIds.Any())
            {
                Db.Delete<Domain.CRM.CampaignProduct>(x => x.CampaignId == request.Id && !Sql.In(x.Id, campaignproductIds));
            }
            else
            {
                Db.Delete<Domain.CRM.CampaignProduct>(x => x.CampaignId == request.Id);
            }

            foreach (var campaignProduct in request.Products)
            {
                if (campaignProduct.Id.HasValue)
                {
                    Db.Update(new Domain.CRM.CampaignProduct
                    {
                        Id = campaignProduct.Id.Value,
                        CampaignId = request.Id,
                        ProductId = campaignProduct.ProductId
                    });
                }
                else
                {
                    Db.Insert(new Domain.CRM.CampaignProduct
                    {
                        CampaignId = request.Id,
                        ProductId = campaignProduct.ProductId
                    });
                }
            }
        }
        public LookupResult Get(Api.LookupCampaignsRequest request)
        {
            var result = this.GetCampaigns(request.Page - 1, request.PageSize, request);
            return new LookupResult
            {
                Data = result.Select(x => new LookupItem
                {
                    Id = x.Id,
                    Text = x.Name
                }),
                Total = result.Count
            };
        }

        private List<Domain.CRM.Campaign> GetCampaigns(int? pageIndex = null, int? pageSize = null, Api.LookupCampaignsRequest request = null)
        {
            var query = Db.From<Domain.CRM.Campaign>();

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                query.Where(x => x.Name.Contains(request.Q));
            }            

            query.And(x => x.TenantId == Session.TenantId);

            var countStatement = query.ToCountStatement();

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query.Limit(pageIndex.Value * pageSize.Value, pageSize.Value);
            }

            query.OrderByDescending(x => x.Id);
            var result = Db.Select(query);
            return result;
        }
    }
}