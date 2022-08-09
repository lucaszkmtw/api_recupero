using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using BusinessDocumentType = CentralOperativa.ServiceModel.BusinessDocuments.BusinessDocumentType;

namespace CentralOperativa.ServiceInterface.BusinessDocuments
{
    [Authenticate]
    public class BusinessDocumentTypeService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(BusinessDocumentType.PostBusinessDocumentTypeRequest request)
        {
            return Db.Update((Domain.BusinessDocuments.BusinessDocumentType)request);
        }

        public object Post(BusinessDocumentType.PostBusinessDocumentTypeRequest request)
        {
            request.Id = (int)Db.Insert((Domain.BusinessDocuments.BusinessDocumentType)request, true);
            return request;
        }

        public object Get(BusinessDocumentType.GetBusinessDocumentTypeRequest request)
        {
            return Db.SingleById<Domain.BusinessDocuments.BusinessDocumentType>(request.Id);
        }

        public object Get(BusinessDocumentType.GetBusinessDocumentTypeParamsRequest request)
        {
            var businessDocumentType = Db.Select<Domain.BusinessDocuments.BusinessDocumentType>(x => x.Code == request.Code).FirstOrDefault();
            return Db.Select<Domain.BusinessDocuments.BusinessDocumentTypeParam>(w => w.TypeId == businessDocumentType.Id).FirstOrDefault();
        }

        public QueryResponse<BusinessDocumentType.QueryBusinessDocumentTypeResult> Get(BusinessDocumentType.QueryBusinessDocumentTypes request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderBy = "Name";
            }

  

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());

            if(request.Module == 3)
            {
                q.Where(w => w.ShortName == "RMS" || w.ShortName == "RME");
            }

            if (request.CollectionDocument == 1)
            {
                q.Where(w => w.CollectionDocument == 1);
            }
            return AutoQuery.Execute(request, q);
        }

        public object Get(BusinessDocumentType.LookupBusinessDocumentTypeRequest request)
        {
            var query = Db.From<Domain.BusinessDocuments.BusinessDocumentType>();
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