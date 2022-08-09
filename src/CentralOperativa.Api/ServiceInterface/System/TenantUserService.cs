using System;
using System.Linq;
using System.Net;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    using TenantUser = ServiceModel.System.TenantUser;

    public class TenantUserService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public TenantUser.PostTenantUser Put(TenantUser.PostTenantUser request)
        {
            Db.Update((Domain.System.TenantUser)request);
            return request;
        }

        public TenantUser.PostTenantUser Post(TenantUser.PostTenantUser request)
        {
            var existing = Db.Select(Db.From<Domain.System.TenantUser>().Where(w => w.TenantId == request.TenantId && w.UserId == request.UserId));
            if (existing.Count > 0)
            {
                throw new HttpError(
                    HttpStatusCode.Conflict, "err.alreadyexists", "El usuario ya está asignado a este propietario");
            }

            request.CreatedById = Session.UserId;
            request.CreateDate = DateTime.UtcNow;
            request.Id = (int)Db.Insert((Domain.System.TenantUser)request, true);
            return request;
        }

        public object Get(TenantUser.QueryTenantUsers request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "PersonName";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public TenantUser.GetTenantUserResponse Get(TenantUser.GetTenantUser request)
        {
            var model = Db.Select<TenantUser.GetTenantUserResponse>(Db
                .From<Domain.System.TenantUser>()
                .Join<Domain.System.User>()
                .Join<Domain.System.User, Domain.System.Persons.Person>()
                .Where(x => x.Id == request.Id)).SingleOrDefault();
            return model;
        }
    }
}