using System.Linq;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.Investments;
using CentralOperativa.Domain.BusinessPartners;
using System.Threading.Tasks;

using Api = CentralOperativa.ServiceModel.Investments;
using ApiSession = CentralOperativa.ServiceModel.System;
using System;

namespace CentralOperativa.ServiceInterface.Investments
{
    [Authenticate]
    public class ManagerService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }


        public object Any(Api.QueryManagers request)
        {

            var query = Db.From<Manager>()
                    .Join<Manager, BusinessPartner>()
                    .Join<BusinessPartner, Person>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            return Db.Select(query);
        }
        public object Put(Api.PostManager request)
        {
            {
                Db.Update((Domain.Investments.Manager)request);

                var bpartner = Db.SingleById<BusinessPartner>(request.BusinessPartnerId);
                bpartner.PersonId = request.PersonId;
                Db.Update((Domain.BusinessPartners.BusinessPartner)bpartner);

                return request;
            }
        }

        public object Post(Api.PostManager request)
        {
            {
                BusinessPartner bpartner = new BusinessPartner();
                bpartner.TenantId = Session.TenantId;
                bpartner.CreatedById = Session.UserId;
                bpartner.Code = "1";
                bpartner.TypeId = 5;
                bpartner.CreateDate = DateTime.UtcNow;
                bpartner.Guid = Guid.NewGuid();
                bpartner.PersonId = request.PersonId;

                var query = Db.From<BusinessPartner>()
                    .Where(w => w.TenantId == Session.TenantId && w.TypeId == 5 && w.PersonId == request.PersonId);

                var businessPartner = Db.Select(query).SingleOrDefault();
                if (businessPartner == null)
                {
                    int bpartnerId = (int)Db.Insert((BusinessPartner)bpartner, true);
                    request.BusinessPartnerId = bpartnerId;
                    request.Id = (int)Db.Insert((Manager)request, true);
                }
                else
                {
                    if (businessPartner.Status == BusinessPartnerStatus.Deleted)
                    {
                        businessPartner.Status = BusinessPartnerStatus.Active;
                        Db.Update((BusinessPartner)businessPartner);

                        var queryManager = Db.From<Manager>().Where(t => t.BusinessPartnerId == businessPartner.Id);
                        var manager = Db.Select(queryManager).SingleOrDefault();
                        manager.Commission = request.Commission;
                        Db.Update((Manager)manager);
                    }
                }
                return request;
            }
        }

        public object Get(Api.GetManager request)
        {
            var manager = Db.SingleById<Domain.Investments.Manager>(request.Id);
            var model = manager.ConvertTo<Api.Manager>();
            return model;
        }

        public QueryResponse<Api.QueryManagersResult> Get(Api.QueryManagers request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where("businessPartners.status = '" + BusinessPartnerStatus.Active.SqlValue() + "'");
            q.Where<BusinessPartner>(w => w.Status == BusinessPartnerStatus.Active);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupManagers request)
        {
            var query = Db.From<Domain.Investments.Manager>()
                        .Join<Manager, BusinessPartner>()
                        .Join<BusinessPartner, Person>();

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetManagerResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

        public object Delete(Api.DeleteManager request)
        {

            var manager = Db.SingleById<Manager>(request.Id);
            var businessPartner = Db.SingleById<BusinessPartner>(manager.BusinessPartnerId);
            businessPartner.Status = BusinessPartnerStatus.Deleted;
            Db.Update((Domain.BusinessPartners.BusinessPartner)businessPartner);

            return request;
        }
    }
}
