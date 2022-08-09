using System.Linq;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.Investments;
using CentralOperativa.Domain.BusinessPartners;

using System.Threading.Tasks;

using Api = CentralOperativa.ServiceModel.Investments;
using ApiSession = CentralOperativa.ServiceModel.System;
using System;

namespace CentralOperativa.ServiceInterface.Investments
{
    [Authenticate]
    public class TraderService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }


        public object Any(Api.QueryTraders request)
        {

            var query = Db.From<Trader>()
                    .Join<Trader, BusinessPartner>()
                    .Join<BusinessPartner, Person>()
                    .Where<BusinessPartner>(w => w.Status == BusinessPartnerStatus.Active)
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            return Db.Select(query);
        }
        public object Put(Api.PostTrader request)
        {
            {

                Db.Update((Domain.Investments.Trader)request);

                var bpartner = Db.SingleById<BusinessPartner>(request.BusinessPartnerId);
                bpartner.PersonId = request.PersonId;
                Db.Update((Domain.BusinessPartners.BusinessPartner)bpartner);

                return request;
            }
        }

        public object Post(Api.PostTrader request)
        {
            {
                BusinessPartner bpartner = new BusinessPartner();
                bpartner.TenantId = Session.TenantId;
                bpartner.CreatedById = Session.UserId;
                bpartner.Code = "1";
                bpartner.TypeId = 4;
                bpartner.CreateDate = DateTime.UtcNow;
                bpartner.Guid = Guid.NewGuid();
                bpartner.PersonId = request.PersonId;

                var query = Db.From<BusinessPartner>()
                    .Where(w => w.TenantId == Session.TenantId && w.TypeId == 4 && w.PersonId == request.PersonId);

                var businessPartner = Db.Select(query).SingleOrDefault();
                if (businessPartner == null)
                {   
                    int bpartnerId = (int)Db.Insert((BusinessPartner)bpartner, true);
                    request.BusinessPartnerId = bpartnerId;
                    request.Id = (int)Db.Insert((Trader)request, true);
                }
                else
                {
                    if (businessPartner.Status == BusinessPartnerStatus.Deleted)
                    {
                        businessPartner.Status = BusinessPartnerStatus.Active;
                        Db.Update((BusinessPartner)businessPartner);
                        
                        var queryTrader = Db.From<Trader>().Where(t => t.BusinessPartnerId == businessPartner.Id);
                        var trader = Db.Select(queryTrader).SingleOrDefault();
                        trader.Commission = request.Commission;
                        Db.Update((Trader)trader);
                    }
                }
                return request;
            }
        }

        public object Get(Api.GetTrader request)
        {
            var trader = Db.SingleById<Domain.Investments.Trader>(request.Id);
            var model = trader.ConvertTo<Api.Trader>();
            return model;
        }

        public QueryResponse<Api.QueryTradersResult> Get(Api.QueryTraders request)
        {
            
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<BusinessPartner>(w => w.Status == BusinessPartnerStatus.Active);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupTraders request)
        {
            var query = Db.From<Domain.Investments.Trader>()
                        .Join<Trader, BusinessPartner>()
                        .Join<BusinessPartner, Person>();

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetTraderResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

        public object Delete(Api.DeleteTrader request)
        {

            var trader = Db.SingleById<Trader>(request.Id);
            var businessPartner = Db.SingleById<BusinessPartner>(trader.BusinessPartnerId);
            businessPartner.Status = BusinessPartnerStatus.Deleted;
            Db.Update((Domain.BusinessPartners.BusinessPartner)businessPartner);

            return request;
        }
    }
}
