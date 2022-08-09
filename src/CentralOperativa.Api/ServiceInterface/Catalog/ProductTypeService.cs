using System.Linq;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Catalog;
using CentralOperativa.Infraestructure;
using Contract = CentralOperativa.ServiceModel.Catalog;

namespace CentralOperativa.ServiceInterface.Catalog
{
    [Authenticate]
    public class ProductTypeService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }
        public object Put(Contract.PostProductType request)
        {
            return Db.Update((ProductType)request);
        }

        public object Post(Contract.PostProductType request)
        {
            request.Id = (int)Db.Insert((ProductType)request, true);
            return request;
        }

        public QueryResponse<Contract.QueryProductTypesResult> Get(Contract.QueryProductTypes request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Contract.LookupProductTypes request)
        {
            var query = Db.From<ProductType>()
                .Select(x => new {x.Id, x.Description});

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Description.Contains(request.Q) || q.Description.Contains(request.Q));
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

        public object Get(Contract.GetProductType request)
        {
            var model = Db.SingleById<ProductType>(request.Id);
            return model;
        }
    }
}