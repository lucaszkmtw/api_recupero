using System;
using System.Linq;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Inv;
using CentralOperativa.Infraestructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralOperativa.ServiceInterface.System;

namespace CentralOperativa.ServiceInterface
{
    [Authenticate]
    public class InventorySiteService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(InventorySiteService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly TenantRepository _tenantRepository;

        public InventorySiteService(IAutoQueryDb autoQuery, TenantRepository tenantRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
        }


        public object Get(Api.QuerySites request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Domain.Inv.InventorySite, Domain.System.Tenant>((iss, t) => iss.PersonId == t.PersonId);
            q.And<Domain.System.Tenant>(w => w.Id == Session.TenantId);
            return _autoQuery.Execute(request, q);
        }


        public object Get(Api.QuerySite request)
        {
            return Db.Select(Db.From<Domain.Inv.InventorySite>().Where(c => c.Id == request.Id))
                .SingleOrDefault()
                .ConvertTo<Api.QuerySite>();
        }

        public async Task<object> Post(Api.PostSite request)
        {
            using (var trx = Db.OpenTransaction())
            {
                var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
                try
                {
                    var site = new Domain.Inv.InventorySite
                    {
                        Name = request.Name,
                        PersonId = tenant.PersonId
                    };
                    request.Id = (int) await Db.InsertAsync(site, true);
                    
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

        public object Put(Api.PostSite request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((Domain.Inv.InventorySite)request);

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

        public LookupResult Get(Api.LookupSite request)
        {
            var q = Db.From<Domain.Inv.InventorySite>();
            if (request.Id.HasValue)
            {
                q.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                q.Where(w => w.PersonId == request.PersonId);
                if (!string.IsNullOrEmpty(request.Q))
                {
                    q.And(w => w.Name.Contains(request.Q));
                }
            }

            var total = Db.Count(q);
            q.OrderByDescending(o => o.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));
            return new LookupResult
            {
                Data = Db.Select(q).Select(x => new LookupItem
                {
                    Id = x.Id,
                    Text = x.Name
                }),
                Total = (int) total
            };
        }

        public LookupResult<Domain.Inv.InventorySite> Get(Api.LookupSiteByPerson request)
        {
            var q = Db.From<Domain.Inv.InventorySite>();
            if (request.Id.HasValue)
            {
                q.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                q.Where(w => w.PersonId == request.PersonId);
            }
            
            var total = Db.Count(q);
            q.OrderByDescending(o => o.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));
            return new LookupResult<Domain.Inv.InventorySite>
            {
                Data = Db.Select(q),
                Total = total
            };
        }

        private async Task<List<Domain.Inv.InventorySite>> GetSites(int? pageIndex = null, int? pageSize = null, Api.LookupSite request = null)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            var query = Db.From<Domain.Inv.InventorySite>();
            query.Where(w => w.PersonId == tenant.PersonId);

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                query.Where(x => x.Name.Contains(request.Q));
            }
            
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