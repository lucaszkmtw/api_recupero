using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Procurement;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Procurement
{
    [Authenticate]
    public class DistributionConditionService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(DistributionCondition.Post request)
        {
            return Db.Update((CentralOperativa.Domain.Procurement.DistributionCondition)request);
        }

        public object Post(DistributionCondition.Post request)
        {
            request.Id = (int)Db.Insert((CentralOperativa.Domain.Procurement.DistributionCondition)request, true);
            return request;
        }

        public object Get(DistributionCondition.Get request)
        {
            var distributionCondition = Db.SingleById<CentralOperativa.Domain.Procurement.DistributionCondition>(request.Id);
            return distributionCondition;
        }

        public QueryResponse<DistributionCondition.QueryResult> Get(DistributionCondition.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(DistributionCondition.Lookup request)
        {
            var query = Db.From<CentralOperativa.Domain.Procurement.DistributionCondition>()
                .Select(x => new { x.Id, x.Order, x.Condition });

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Condition.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Condition }),
                Total = (int)count
            };
            return result;
        }
    }
}