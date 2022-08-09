using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Financials;

namespace CentralOperativa.ServiceInterface.Financials
{
    [Authenticate]
    public class PaymentDocumentTypeService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostPaymentDocumentTypeRequest request)
        {
            return Db.Update((Domain.Financials.PaymentDocumentType)request);
        }

        public object Post(Api.PostPaymentDocumentTypeRequest request)
        {
            request.Id = (int)Db.Insert((Domain.Financials.PaymentDocumentType)request, true);
            return request;
        }

        public object Get(Api.GetPaymentDocumentTypeRequest request)
        {
            return Db.SingleById<Domain.Financials.PaymentDocumentType>(request.Id);
        }

        public QueryResponse<Api.QueryPaymentDocumentTypeResult> Get(Api.QueryPaymentDocumentTypes request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderBy = "Name";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupPaymentDocumentTypeRequest request)
        {
            var query = Db.From<Domain.Financials.PaymentDocumentType>();
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
                query.Where(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}