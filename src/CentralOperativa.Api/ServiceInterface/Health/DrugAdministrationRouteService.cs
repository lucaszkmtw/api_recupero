using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class DrugAdministrationRouteService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(DrugAdministrationRoute.Post request)
        {
            return Db.Update((Domain.Health.DrugAdministrationRoute)request);
        }

        public object Post(DrugAdministrationRoute.Post request)
        {
            request.Id = (short)Db.Insert((Domain.Health.DrugAdministrationRoute)request, true);
            return request;
        }

        public object Get(DrugAdministrationRoute.Get request)
        {
            var drug = Db.SingleById<Domain.Health.DrugAdministrationRoute>(request.Id);
            return drug;
        }

        public QueryResponse<DrugAdministrationRoute.QueryResult> Get(DrugAdministrationRoute.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(DrugAdministrationRoute.Lookup request)
        {
            var query = Db.From<Domain.Health.DrugAdministrationRoute>();
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
    }
}