using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    using Api = ServiceModel.System.Permission;

    [Authenticate]
    public class PermissionServices : Service
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.GetPermission request)
        {
            return Db.SingleById<Domain.System.Permission>(request.Id);
        }

        public QueryResponse<Domain.System.Permission> Get(Api.QueryPermissions request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            if (string.IsNullOrEmpty(request.OrderBy))
            {
                query.OrderBy(x => x.Name);
            }

            return AutoQuery.Execute(request, query);
        }

        public LookupResult Get(Api.LookupPermission request)
        {
            var query = Db.From<Domain.System.Permission>();
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }

            else
            {
                if (!string.IsNullOrEmpty(request.Filter))
                {
                    /*
                    if (request.Filter == "with_no_linked_user")
                    {
                        var userPersonsIds = Db.Column<int>(Db.From<Domain.System.User>().Select(x => x.PersonId));
                        query.And(w => !Sql.In(w.Id, userPersonsIds));
                    }
                    */
                }

                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        query.Where(x => x.Name.Contains(token));
                    }
                }
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .OrderBy(x => x.Name)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }

        public object Delete(Api.DeletePermission request)
        {
            return Db.DeleteById<Domain.System.Permission>(request.Id);
        }

        public Api.PostPermission Post(Api.PostPermission request)
        {
            //verificar si ya existe
            var permCheck = Db.Exists<Domain.System.Permission>(x => x.Name == request.Name);
            if (!permCheck)
            {
                request.Id = (int)Db.Insert((Domain.System.Permission)request, true);
                return request;
            }

            throw new ServiceResponseException("Permission");
        }

        public Api.PostPermission Put(Api.PostPermission request)
        {
            var checkExist = Db.Exists<Domain.System.Permission>(x => x.Name == request.Name);
            if (!checkExist)
            {
                Db.Update<Domain.System.Permission>(request);
                return request;
            }

            throw new HttpError(global::System.Net.HttpStatusCode.NotModified, "304");
        }

        public Domain.System.RolePermission Get(Api.GetRolePermission request)
        {
            return Db.SingleById<Domain.System.RolePermission>(request.RolePermissionId);
        }

        public Domain.System.UserPermission Get(Api.GetUserPermission request)
        {
            return Db.SingleById<Domain.System.UserPermission>(request.UserPermissionId);
        }

        public QueryResponse<Api.QueryRolePermissionResult> Get(Api.GetRolesByPermission request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.System.Role, Domain.System.RolePermission>((r, p) => r.Id == p.RoleId && p.PermissionId == request.PermissionId);
            return AutoQuery.Execute(request, q);
        }

        public QueryResponse<Api.QueryUserPermissionResult> Get(Api.GetUsersByPermission request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.System.User, Domain.System.UserPermission>((u, p) => u.Id == p.UserId && p.PermissionId == request.PermissionId);
            return AutoQuery.Execute(request, q);
        }

        public Api.PostUserPermission Post(Api.PostUserPermission request)
        {
            request.Id = (byte)Db.Insert<Domain.System.UserPermission>(request);
            return request;
        }

        public object Put(Api.PostUserPermission request)
        {
            return Db.Delete<Domain.System.UserPermission>(request);
        }

        public object Post(Api.PostRolePermission request)
        {
            request.Id = (byte)Db.Insert<Domain.System.RolePermission>(request);
            return request;
        }

        public object Put(Api.PostRolePermission request)
        {
            return Db.Delete<Domain.System.RolePermission>(request);
        }
    }
}