using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class DrugService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Drug.Post Put(Drug.Post request)
        {
            Db.Update((Domain.Health.Drug)request);
            return request;
        }

        public Drug.Post Post(Drug.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Health.Drug)request, true);
            return request;
        }

        public Domain.Health.Drug Get(Drug.Get request)
        {
            var drug = Db.SingleById<Domain.Health.Drug>(request.Id);
            return drug;
        }

        public QueryResponse<Drug.QueryResult> Get(Drug.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var p = Request.GetRequestParams();
            var q = AutoQuery.CreateQuery(request, p);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Drug.Lookup request)
        {
            var query = Db.From<Domain.Health.Drug>()
                .Select(x => new { x.Id, x.Name });

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
    }
}