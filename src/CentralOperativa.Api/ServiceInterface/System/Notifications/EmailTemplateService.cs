using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System.Notifications;

namespace CentralOperativa.ServiceInterface.System.Notifications
{
    [Authenticate]
    public class EmailTemplateService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostEmailTemplate request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((Domain.System.Notifications.EmailTemplate) request);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public Api.PostEmailTemplate Post(Api.PostEmailTemplate request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.Select(Db.From<Domain.System.Notifications.EmailTemplate>().Where(w => w.Name == request.Name));
                    if (existing.Count > 0)
                    {
                        throw new ApplicationException("There is already a template with that name.");
                    }

                    request.Id = (int)Db.Insert((Domain.System.Notifications.EmailTemplate)request, true);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Get(Api.GetEmailTemplate request)
        {
            return Db.SingleById<Domain.System.Notifications.EmailTemplate>(request.Id);
        }

        public QueryResponse<Api.QueryEmailTemplatesResponse> Get(Api.QueryEmailTemplates request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupEmailTemplate request)
        {
            var query = Db.From<Domain.System.Notifications.EmailTemplate>();
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Notifications.EmailTemplate>(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query)
                .Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}
