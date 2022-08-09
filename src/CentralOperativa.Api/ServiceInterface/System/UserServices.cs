using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CentralOperativa.Domain.System;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceInterface.System
{
    using Api = ServiceModel.System;
    public class UserServices : ApplicationService
    {
        private IAutoQueryDb _autoQuery;
        private UserRepository _userRepository;

        public UserServices(IAutoQueryDb autoQuery, UserRepository userRepository)
        {
            _autoQuery = autoQuery;
            _userRepository = userRepository;
        }

        public LookupResult Get(Api.LookupUser request)
        {
            var query = Db.From<User>()
                .Join<Domain.System.Persons.Person>();

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
                        var userPersonsIds = Db.Column<int>(Db.From<User>().Select(x => x.PersonId));
                        query.And(w => !Sql.In(w.Id, userPersonsIds));
                    }
                }

                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        query
                            .Where(x => x.Name.Contains(token))
                            .Or<Domain.System.Persons.Person>(w => w.Name.Contains(token));
                    }
                }
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryUsersResponse>(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name + " (" + x.PersonName + ")" }),
                Total = (int)count
            };
            return result;
        }

        public async Task<Api.User> Get(Api.GetUser request)
        {
            return await _userRepository.GetUser(Db, request.Id);
        }

        public Api.QueryUsersResponse Put(Api.PostUser request)
        {
            var current = Db.SingleById<User>(request.Id);
            current.PopulateWith(request);
            Db.Save(current);
            return current.ConvertTo<Api.QueryUsersResponse>();
        }

        public Api.QueryUsersResponse Post(Api.PostUser request)
        {
            var nameExists = Db.Exists<User>(w => w.Name == request.Name);
            if (!nameExists)
            {
                var user = Db.Select(Db.From<User>().Where(w => w.PersonId == request.PersonId)).SingleOrDefault();
                if (user == null)
                {
                    user = new User
                    {
                        PersonId = request.PersonId,
                        Name = request.Name,
                        Password = request.Password
                    };
                    user.Id = (int) Db.Insert(user, true);

                    //Assign the user to the current tenant
                    var tenantUser = new TenantUser
                    {
                        CreateDate = DateTime.UtcNow,
                        CreatedById = Session.UserId,
                        TenantId = Session.TenantId,
                        UserId = user.Id
                    };
                    Db.Insert(tenantUser);
                }

                //TODO: Si existe devolver Conflict

                return user.ConvertTo<Api.QueryUsersResponse>();
            }

            throw new HttpError(HttpStatusCode.Conflict, "err.user.usernamealreadyexists", "Ya existe un usuario con este nombre");
        }

        public QueryResponse<Api.QueryUsersResponse> Get(Api.QueryUsers request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());

            // Filter only current user's tenant users
            q.Join<User, TenantUser>();
            q.Where<TenantUser>(w => w.TenantId == Session.TenantId);

            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.GetUserRole request)
        {
            return Db.SingleById<UserRole>(request.UserRoleId);
        }

        public object Get(Api.GetUserPermission request)
        {
            return Db.SingleById<UserPermission>(request.UserPermissionId);
        }

        public List<Api.QueryUserRoleResult> Get(Api.GetRolesByUser request)
        {
            var result = Db.Select<Api.QueryUserRoleResult>(Db
               .From<Role>()
               .LeftJoin<Role, UserRole>((x, y) => y.UserId == request.Id && x.Id == y.RoleId)
               .Where(w => w.TenantId == Session.TenantId));
            return result;

        }

        public List<Api.QueryUserPermissionResult> Get(Api.GetPermissionsByUser request)
        {
            var result = Db.Select<Api.QueryUserPermissionResult>(Db
                .From<Permission>()
                .LeftJoin<Permission, UserPermission>(
                    (x, y) => x.Id == y.PermissionId && y.UserId == request.Id));
            return result;
        }

        public object Post(Api.PostUserPermission request)
        {
            throw new NotImplementedException();
            /*
                var tentantUserId = tenantUsers[0].Id;
                request.TenantUserId = tentantUserId;
                request.Id = (byte)Db.Insert<Domain.System.UserPermission>(request);
                return request;
                */
        }

        public object Put(Api.PostUserPermission request)
        {
            return Db.Delete<UserPermission>(request);
        }

        public object Post(Api.PostUserRole request)
        {
            request.Id = (byte)Db.Insert<UserRole>(request);
            return request;
        }

        public object Put(Api.PostUserRole request)
        {
            return Db.Delete<UserRole>(request);
        }
    }
}
