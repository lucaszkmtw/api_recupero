using System.Linq;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class CheckBookServices : ApplicationService
    {
        public object Put(Api.PostCheckBook request)
        {
            return Db.Update((CheckBook)request);
        }

        public object Post(Api.PostCheckBook request)
        {
            request.Id = (int)Db.Insert((CheckBook)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public QueryResponse<Api.QueryCheckBooksResult> Get(Api.QueryCheckBooks request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<BankAccount>(x => x.PersonId == request.PersonId);
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupCheckBook request)
        {
            var query = Db.From<CheckBook>()
                .Join<CheckBook, BankAccount>()
                .Join<BankAccount, BankBranch>()
                .Join<BankBranch, Bank>()
                .Where<BankAccount>(x => x.PersonId == request.PersonId);

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(q => Sql.In(q.Id, request.Ids));
            }


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetCheckBookResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.BankName + " - " + x.BankAccountCode + " - " + x.Name }),
                Total = (int)count
            };
            return result;
        }

        public object Get(Api.GetCheckBook request)
        {
            var model = Db.SingleById<CheckBook>(request.Id);
            return model;
        }
    }
}