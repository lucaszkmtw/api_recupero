using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class BankService : ApplicationService
    {
        public object Put(Bank.PostBank request)
        {
            return Db.Update((Domain.Financials.Bank)request);
        }

        public object Post(Bank.PostBank request)
        {
            request.Id = (int)Db.Insert((Domain.Financials.Bank)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Bank.QueryBanks request)
        {
            var query = Db.From<Domain.Financials.Bank>()
                .OrderByDescending(q => q.Id)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                query.Where(q => q.Code.Contains(request.Code));

            return Db.Select(query);
        }

        public LookupResult Get(Bank.LookupBank request)
        {
            var query = Db.From<Domain.Financials.Bank>()
                .Select(x => new {x.Id, x.Name});

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

        public object Get(Bank.GetBank request)
        {
            var model = Db.SingleById<Domain.Financials.Bank>(request.Id);
            return model;
        }
    }
}