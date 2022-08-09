using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Cms;

namespace CentralOperativa.ServiceInterface.Cms
{
    [Authenticate]
    public class ContentService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.GetContent request)
        {
            return Db.Single<Domain.Cms.Content>(w => w.Id == request.Id);
        }

        public object Get(Api.QueryContents request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(w => w.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupContents request)
        {
            var query = Db.From<Domain.Cms.Content>()
                .Where(w => w.TenantId == Session.TenantId)
                .Select(x => new { x.Id, x.Title });

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(w => Sql.In(w.Id, request.Ids));
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Title.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Title }),
                Total = (int)count
            };
            return result;
        }

        [Authenticate]
        public object Post(Api.PostContent request)
        {
            request.CreateDate = DateTime.UtcNow;
            request.LastEditDate = DateTime.UtcNow;
            request.TenantId = Session.TenantId;
            request.Id = (int) Db.Insert((Domain.Cms.Content) request, true);
            return request;
        }

        [Authenticate]
        public object Put(Api.PostContent request)
        {
            request.LastEditDate = DateTime.UtcNow;
            Db.Update((Domain.Cms.Content)request);
            return request;
        }

        [Authenticate]
        public void Delete(Api.DeleteContent request)
        {
            Db.DeleteById<Domain.Cms.Content>(request.Id);
        }
    }
}
