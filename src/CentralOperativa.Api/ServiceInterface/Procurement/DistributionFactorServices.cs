using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Procurement;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Procurement
{
    [Authenticate]
    public class DistributionFactorServices : ApplicationService
    {
        public object Put(DistributionFactor.Post request)
        {
            return Db.Update((CentralOperativa.Domain.Procurement.DistributionFactor)request);
        }

        public object Post(DistributionFactor.Post request)
        {
            request.Id = (int)Db.Insert((CentralOperativa.Domain.Procurement.DistributionFactor)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public QueryResponse<DistributionFactor.QueryDistributionFactorsResult> Get(DistributionFactor.QueryDistributionFactors request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Description";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(DistributionFactor.Lookup request)
        {
            var query = Db.From<CentralOperativa.Domain.Procurement.DistributionFactor>()
                .Select(x => new {x.Id, x.Code, x.Description});

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Code.Contains(request.Q) || q.Description.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page.GetValueOrDefault(0), request.PageSize.GetValueOrDefault(10) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Code + " - " + x.Description }),
                Total = (int)count
            };
            return result;
        }

        public object Get(DistributionFactor.GetDistributionFactor request)
        {
            var model = Db.SingleById<CentralOperativa.Domain.Procurement.DistributionFactor>(request.Id);
            return model;
        }
    }
}