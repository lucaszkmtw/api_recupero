using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class PharmacyService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Pharmacy.Post request)
        {
            return Db.Update((Domain.Health.Pharmacy)request);
        }

        public object Post(Pharmacy.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Health.Pharmacy)request, true);
            return request;
        }

        public object Get(Pharmacy.Get request)
        {
            var pharmacy = Db.SingleById<Domain.Health.Pharmacy>(request.Id);
            return pharmacy;
        }

        public QueryResponse<Pharmacy.QueryResult> Get(Pharmacy.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Pharmacy.Lookup request)
        {
            var query = Db.From<Domain.Health.Pharmacy>();
            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Description.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Description }),
                Total = (int)count
            };
            return result;
        }
    }
}