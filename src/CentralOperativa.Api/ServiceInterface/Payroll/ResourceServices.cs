using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Operation = CentralOperativa.ServiceModel.HumanResources.Employee;

namespace CentralOperativa.ServiceInterface.Payroll
{
    [Authenticate]
    public class ResourceServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Operation.Put request)
        {
            return Db.Update((Domain.HumanResources.Employee)request);
        }

        public object Post(Operation.Post request)
        {
            request.Id = (int)Db.Insert((Domain.HumanResources.Employee)request, true);
            return request;
        }

        public Domain.HumanResources.Employee Get(Operation.Get request)
        {
            return Db.SingleById<Domain.HumanResources.Employee>(request.Id);
        }

        public QueryResponse<Operation.QueryResult> Get(Operation.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var requestParams = Request.GetRequestParams();
            var q = AutoQuery.CreateQuery(request, requestParams);
            if (requestParams.ContainsKey("fullName"))
            {
                var nameFilter = requestParams["fullName"];
                q.Where(x =>  x.FirstName.Contains(nameFilter) || x.LastName.Contains(nameFilter));
            }
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Operation.Lookup request)
        {
            var query = Db.From<Domain.HumanResources.Employee>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.LastName.Contains(request.Q) || q.FirstName.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderByDescending(q => q.Id)
            .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.FullName }),
                Total = (int)count
            };
            return result;
        }
    }
}