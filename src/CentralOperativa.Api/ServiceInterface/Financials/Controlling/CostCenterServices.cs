using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Financials.Controlling;
using CentralOperativa.Infraestructure;
using Api = CentralOperativa.ServiceModel.Financials.Controlling;

namespace CentralOperativa.ServiceInterface.Financials.Controlling
{
    [Authenticate]
    public class CostCenterService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly CostCenterRepository _costCenterRepository;

        public CostCenterService(
            IAutoQueryDb autoQuery,
            CostCenterRepository costCenterRepository)
        {
            _autoQuery = autoQuery;
            _costCenterRepository = costCenterRepository;
        }

        public async Task<Api.CostCenter> Get(Api.GetCostCenter request)
        {
            return await _costCenterRepository.GetCostCenter(Db, request.Id);
        }

        public Api.CostCenter Put(Api.PostCostCenter request)
        {
            var data = Db.SingleById<CostCenter>(request.Id);
            if (data.TenantId != Session.TenantId)
            {
                throw HttpError.Unauthorized(string.Empty);
            }

            data.PopulateWith(request);
            Db.Update(data);
            return request;
        }

        public Api.CostCenter Post(Api.PostCostCenter request)
        {
            var data = request.ConvertTo<CostCenter>();
            data.TenantId = Session.TenantId;
            request.Id = (int)Db.Insert(data, true);
            return request;
        }

        public void Delete(Api.DeleteCostCenter request)
        {
            var data = Db.Select(Db.From<CostCenter>().Where(w => w.TenantId == Session.TenantId && w.Id == request.Id)).SingleOrDefault();
            if(data != null)
            {
                Db.Delete(data);
            }
        }

        public QueryResponse<Api.QueryCostCentersResult> Any(Api.QueryCostCenters request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var p = Request.GetRequestParams();
            var q = _autoQuery.CreateQuery(request, p);
            q.Where(w => w.TenantId == Session.TenantId);
            return _autoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupCostCenter request)
        {
            var query = Db.From<CostCenter>()
                .Select(x => new { x.Id, x.Name });

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Name.Contains(request.Q) || q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}