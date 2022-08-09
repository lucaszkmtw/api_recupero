using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.ServiceModel.Sales;

namespace CentralOperativa.ServiceInterface.Sales
{
    [Authenticate]
    public class SalesProductCatalogServices : ApplicationService
    {
        public object Put(ProductCatalog.Post request)
        {
            return Db.Update((Domain.Sales.SalesProductCatalog)request);
        }

        public object Post(ProductCatalog.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Sales.SalesProductCatalog)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(ProductCatalog.Find request)
        {
            var query = Db.From<Domain.Sales.SalesProductCatalog>()
                .OrderByDescending(q => q.Id)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            return Db.Select(query);
        }

        public LookupResult Get(ProductCatalog.Lookup request)
        {
            var query = Db.From<Domain.Sales.SalesProductCatalog>()
                .Select(x => new {x.Id, x.Name});
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

        public object Get(ProductCatalog.Get request)
        {
            var model = Db.SingleById<CentralOperativa.Domain.Sales.SalesProductCatalog>(request.Id);
            return model;
        }
    }
}