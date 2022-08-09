using System;
using ServiceStack;
using ServiceStack.OrmLite;
using System.Linq;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceInterface.System
{
    using Api = ServiceModel.System;

    [Authenticate]
    public class RoleServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public LookupResult Get(Api.LookupRole request)
        {
            var query = Db.From<Domain.System.Role>();
            query.Where(w => w.TenantId == Session.TenantId);

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }

            else
            {
                if (!string.IsNullOrEmpty(request.Filter))
                {
                    if (request.Filter == "with_no_linked_user")
                    {
                        var userPersonsIds = Db.Column<int>(Db.From<Domain.System.User>().Select(x => x.PersonId));
                        query.And(w => !Sql.In(w.Id, userPersonsIds));
                    }
                }

                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        int intToken;
                        if (int.TryParse(token, out intToken))
                        {
                            query.Where(x => x.Name.Contains(token) || x.Description.Contains(token));
                        }
                        else
                        {
                            query.Where(x => x.Name.Contains(token));
                        }
                    }
                }
            }

            var count = Db.Count(query);

            //query = query.OrderByDescending(q => q.Id)
            //    .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            query = query.OrderBy(q => q.ListIndex)
            .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }

        public object Get(Api.GetRole request)
        {
            return Db.SingleById<Domain.System.Role>(request.Id);
        }

        public object Post(Api.PostRole request)
        {
            var roleExists = Db.Exists<Domain.System.Role>(x => x.Name == request.Name && x.TenantId == Session.TenantId);
            if (!roleExists)
            {
                var role = request.ConvertTo<Domain.System.Role>();
                role.TenantId = Session.TenantId;
                role.Id = (int)Db.Insert(role, true);
                return role;
            }

            throw new ServiceResponseException("Role");
        }

        public object Put(Api.PostRole request)
        {
            var checkExist = Db.Exists<Domain.System.Role>(w => w.Name == request.Name && w.TenantId == Session.TenantId);
            if (!checkExist)
            {
                var valid = request.ConvertTo<Domain.System.Role>();
                return Db.Update(valid);
            }

            return new HttpError(global::System.Net.HttpStatusCode.NotModified, "304");
        }

        public object Delete(Api.DeleteRole request)
        {
            return Db.DeleteById<Domain.System.Role>(request.Id);
        }

        public QueryResponse<Domain.System.Role> Get(Api.QueryRoles request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            query.Where(x => x.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, query);
        }

        #region RoleUsers
        public object Get(Api.GetRoleUser request)
        {
            return Db.Select<Domain.System.UserRole>(w => w.RoleId == request.RoleId && w.UserId == request.UserId).SingleOrDefault();
        }

        public QueryResponse<Api.QueryRoleUserResult> Get(Api.GetUsersByRole request)
        {
            var queryParameters = Request.GetRequestParams();
            if (queryParameters.ContainsKey("userName"))
            {
                queryParameters["userNameContains"] = queryParameters["userName"];
                queryParameters.Remove("userName");
            }

            var query = AutoQuery.CreateQuery(request, queryParameters);
            switch (request.View)
            {
                case 0:
                    break;
                case 1:
                    // Only Assigned
                    query.UnsafeWhere("UserRoles.Id IS NOT NULL");
                    break;
                case 2:
                    // Only Unasigned
                    query.UnsafeWhere("UserRoles.Id IS NULL");
                    break;
            }
            query.WhereExpression = query.WhereExpression.Replace("WHERE \"UserRoles\".\"RoleId\" = @0", string.Empty);
            if (!string.IsNullOrEmpty(query.WhereExpression) && query.WhereExpression.StartsWith(" AND"))
            {
                query.WhereExpression = "WHERE " + query.WhereExpression.Substring(4);
            }
            query.FromExpression = Db
                .From<Domain.System.User>()
                .Join<Domain.System.User, Domain.System.TenantUser>()
                .LeftJoin<Domain.System.User, Domain.System.UserRole>((u, ur) => u.Id == ur.UserId && ur.RoleId == request.RoleId).FromExpression;
            query.Where<Domain.System.TenantUser>(w => w.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, query);
        }

        public bool Post(Api.PostRoleUser request)
        {
            if (request.UserIds.Length > 0)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        foreach (var userId in request.UserIds)
                        {
                            var userRole = new Domain.System.UserRole { RoleId = request.RoleId, UserId = userId };
                            Db.Insert(userRole);
                        }

                        trx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        return false;
                    }
                }
            }

            return false;
        }

        public bool Post(Api.DeleteUserRole request)
        {
            if (request.UserIds.Length > 0)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        Db.Delete<Domain.System.UserRole>(w => w.RoleId == request.RoleId && Sql.In(w.UserId, request.UserIds));
                        trx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        return false;
                    }
                }
            }

            return false;
        }
        #endregion

        #region RolePermissions
        public Domain.System.RolePermission Get(Api.GetRolePermission request)
        {
            return Db.Select<Domain.System.RolePermission>(w => w.RoleId == request.RoleId && w.PermissionId == request.PermissionId).SingleOrDefault();
        }

        public QueryResponse<Api.QueryRolePermissionResult> Get(Api.GetRolePermissions request)
        {
            var queryParameters = Request.GetRequestParams();
            if (queryParameters.ContainsKey("permissionName"))
            {
                queryParameters["permissionNameContains"] = queryParameters["permissionName"];
                queryParameters.Remove("permissionName");
            }

            var query = AutoQuery.CreateQuery(request, queryParameters);
            switch (request.View)
            {
                case 0:
                    break;
                case 1:
                    // Only Assigned
                    query.UnsafeWhere("RolePermissions.Id IS NOT NULL");
                    break;
                case 2:
                    // Only Unasigned
                    query.UnsafeWhere("RolePermissions.Id IS NULL");
                    break;
            }
            query.WhereExpression = query.WhereExpression.Replace("WHERE \"RolePermissions\".\"RoleId\" = @0", string.Empty);
            if (!string.IsNullOrEmpty(query.WhereExpression) && query.WhereExpression.StartsWith(" AND"))
            {
                query.WhereExpression = "WHERE " + query.WhereExpression.Substring(4);
            }
            query.FromExpression = Db.From<Domain.System.Permission>().LeftJoin<Domain.System.Permission, Domain.System.RolePermission>((p, rp) => p.Id == rp.PermissionId && rp.RoleId == request.RoleId).FromExpression;
            query.OrderBy(x => x.Name);
            return AutoQuery.Execute(request, query);
        }

        public bool Post(Api.PostRolePermission request)
        {
            if (request.PermissionIds.Length > 0)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        foreach (var permissionId in request.PermissionIds)
                        {
                            var rolePermission = new Domain.System.RolePermission { RoleId = request.RoleId, PermissionId = permissionId };
                            Db.Insert(rolePermission);
                        }

                        trx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        return false;
                    }
                }
            }

            return false;
        }

        public bool Post(Api.DeleteRolePermission request)
        {
            if (request.PermissionIds.Length > 0)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        Db.Delete<Domain.System.RolePermission>(w => w.RoleId == request.RoleId && Sql.In(w.PermissionId, request.PermissionIds));
                        trx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        return false;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}