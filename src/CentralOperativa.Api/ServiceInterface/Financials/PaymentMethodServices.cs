using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.Financials;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class PaymentMethodServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostPaymentMethod request)
        {
            return Db.Update((PaymentMethod)request);
        }

        public object Post(Api.PostPaymentMethod request)
        {
            request.TenantId = Session.TenantId;
            request.Id = (int)Db.Insert((PaymentMethod)request, true);
            return request;
        }

        public QueryResponse<Api.QueryPaymentMethodsResult> Get(Api.QueryPaymentMethods request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupPaymentMethod request)
        {
            var query = Db.From<PaymentMethod>()
                .Select(x => new {x.Id, x.Name});

			if (request.Id.HasValue){
				query = query.Where(w => w.Id == request.Id.Value);
            }

            if (!string.IsNullOrEmpty(request.Q)){
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

        public object Get(Api.GetPaymentMethod request)
        {
            var model = Db.SingleById<PaymentMethod>(request.Id);
            return model;
        }
    }
}