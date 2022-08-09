using System;
using System.Linq;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Infraestructure;
using Api = CentralOperativa.ServiceModel.Projects;

namespace CentralOperativa.ServiceInterface.Projects
{
    [Authenticate]
    public class CategoryService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Api.Category Get(Api.GetCategory request)
        {
            var category = Db.SingleById<Domain.Projects.Category>(request.Id).ConvertTo<Api.Category>();
            return category;
        }

        public QueryResponse<Api.Category> Get(Api.QueryCategories request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var p = Request.GetRequestParams();
            var q = AutoQuery.CreateQuery(request, p);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupCategories request)
        {
            var query = Db.From<Domain.Projects.Category>();
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(q => Sql.In(q.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }

        public object Post(Api.PostCategory request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.Select(Db.From<Domain.Projects.Category>().Where(w => w.Name != request.Name));
                    if (existing.Count > 0)
                    {
                        trx.Rollback();
                        return HttpError.Conflict("ERR_ProjectCategory_AlreadyExists");
                    }
                    request.Id = (int)Db.Insert((Domain.Projects.Category)request, true);
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

        public object Put(Api.PostCategory request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((Domain.Projects.Category)request);
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
    }
}