using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class BankBranchBranchServices : ApplicationService
    {
        public object Put(BankBranch.Post request)
        {
            return Db.Update((Domain.Financials.BankBranch)request);
        }

        public object Post(BankBranch.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Financials.BankBranch)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        //public object Any(BankBranch.Find request)
        //{
        //    var query = Db.From<Domain.Financials.BankBranch>()
        //        .OrderByDescending(q => q.Id)
        //        .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

        //    if (!string.IsNullOrEmpty(request.Name))
        //        query.Where(q => q.Name.Contains(request.Name));

        //    return Db.Select(query);
        //}

          public QueryResponse<BankBranch.QueryResult> Get(BankBranch.Find request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(BankBranch.Lookup request)
        {
            var query = Db.From<Domain.Financials.BankBranch>()
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

        public object Get(BankBranch.Get request)
        {
            var model = Db.SingleById<CentralOperativa.Domain.Financials.BankBranch>(request.Id);
            return model;
        }
    }
}