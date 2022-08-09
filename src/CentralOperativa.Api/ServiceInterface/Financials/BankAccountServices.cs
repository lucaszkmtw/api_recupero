using System.Linq;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class BankAccountAccountServices : ApplicationService
    {
        public object Put(Api.PostBankAccount request)
        {
            return Db.Update((BankAccount)request);
        }

        public object Post(Api.PostBankAccount request)
        {
            request.PersonId = request.PersonId;
            request.Id = (int)Db.Insert((BankAccount)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

         public QueryResponse<Api.QueryBankAccountResult> Get(Api.QueryBankAccounts request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());

            
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupBankAccount request)
        {
            var query = Db.From<BankAccount>()
                .Join<BankAccount, BankBranch>()
                .Join<BankBranch, Bank>()
                .Where(w => w.PersonId == request.PersonId);

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Code.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);

           query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetBankAccountResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.BankName + "-" + x.Code }),
                Total = (int)count
            };
            return result;
        }

        public object Get(Api.GetBankAccount request)
        {
            var model = Db.SingleById<BankAccount>(request.Id);
            return model;
        }
    }
}