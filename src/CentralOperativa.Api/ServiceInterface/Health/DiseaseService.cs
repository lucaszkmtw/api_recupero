using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class DiseaseService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Disease.Post request)
        {
            return Db.Update((Domain.Health.Disease)request);
        }

        public object Post(Disease.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Health.Disease)request, true);
            return request;
        }

        public async Task<Disease.GetResponse> Get(Disease.Get request)
        {
            var disease = (await Db.SelectAsync<Disease.GetResponse>(Db.From<Domain.Health.Disease>()
                .Join<Domain.Health.Disease, Domain.Health.DiseaseFamily>()
                .Join<Domain.Health.Disease, Domain.Health.DiseaseGroup>()
                .Where(w => w.Id == request.Id))).SingleOrDefault();
            return disease;
        }

        public QueryResponse<Disease.QueryResult> Get(Disease.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Disease.Lookup request)
        {
            var query = Db.From<Domain.Health.Disease>();

            if (request.Ids != null)
            {
                query.Where(w => Sql.In(w.Id, request.Ids));
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderBy(q => q.Code)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = $"({x.Code}) {x.Name}"}),
                Total = (int)count
            };
            return result;
        }
    }
}