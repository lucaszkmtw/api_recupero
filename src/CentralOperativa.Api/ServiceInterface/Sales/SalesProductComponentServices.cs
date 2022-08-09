using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.ServiceModel.Sales;

namespace CentralOperativa.ServiceInterface.Sales
{
    [Authenticate]
    public class SalesProductComponentServices : ApplicationService
    {
        public object Put(ProductComponent.Post request)
        {
            return Db.Update((Domain.Sales.SalesProductComponent)request);
        }

        public object Post(ProductComponent.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Sales.SalesProductComponent)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(ProductComponent.Find request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(ProductComponent.Lookup request)
        {
            var query = Db.From<Domain.Sales.SalesProductComponent>()
                .Select(x => new {x.Id, x.Quantity});
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(q => Sql.In(q.Id, request.Ids));
            }
            //if (!string.IsNullOrEmpty(request.Q))
            //{
            //    query = query.Where(q => q.Name.Contains(request.Q) || q.Name.Contains(request.Q));
            //}

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Id.ToString() }),
                Total = (int)count
            };
            return result;
        }

        public object Get(ProductComponent.Get request)
        {
            var model = Db.SingleById<CentralOperativa.Domain.Sales.SalesProductComponent>(request.Id);
            return model;
        }
    }
}