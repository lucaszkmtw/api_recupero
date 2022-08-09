using System.Collections.Generic;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    [Route("/system/users/lookup", "GET")]
    public class LookupUser : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/system/users/{Id}", "GET")]
    public class GetUser
    {
        public int Id { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public int PersonId { get; set; }

        public Persons.Person Person { get; set; }

        public int? FolderId { get; set; }

        public string Name { get; set; }
    }

    [Route("/system/users", "GET")]
    public class QueryUsers : QueryDb<Domain.System.User, QueryUsersResponse>
        , IJoin<Domain.System.User, Person>
    {
    }

    public class QueryUsersResponse
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string Name { get; set; }
    }

    [Route("/system/users", "POST")]
    [Route("/system/users/{Id}", "PUT")]
    public class PostUser : QueryUsersResponse
    {
        public string Password { get; set; }
    }

    [Route("/system/users/role/{UserRoleId}", "GET")]
    public class GetUserRole : Domain.System.UserPermission
    {
        public int UserRoleId { get; set; }
    }

    [Route("/system/users/{Id}/roles", "GET")]
    public class GetRolesByUser : QueryDb<Domain.System.User, QueryUserRoleResult>
        , IJoin<Domain.System.User, Domain.System.UserRole>
        , IJoin<Domain.System.UserRole, Domain.System.Role>
    {
        public int Id { get; set; }
    }

    [Route("/system/users/role", "POST")]
    [Route("/system/users/role/{UserRoleId}", "PUT")]
    public class PostUserRole : Domain.System.UserRole
    {
        public int UserRoleId { get; set; }
    }

    [Route("/system/users/permission", "POST")]
    [Route("/system/users/permission/{UserPermissionId}", "PUT")]
    public class PostUserPermission : Domain.System.UserPermission
    {
        public int UserPermissionId { get; set; }
    }

    [Route("/system/users/permission/{UserPermissionId}", "GET")]
    public class GetUserPermission : Domain.System.UserPermission
    {
        public int UserPermissionId { get; set; }
    }

    [Route("/system/users/{Id}/permissions", "GET")]
    public class GetPermissionsByUser : QueryDb<Domain.System.Permission, QueryUserPermissionResult>
        , IJoin<Domain.System.Permission, Domain.System.UserPermission>
        , IJoin<Domain.System.UserPermission, Domain.System.User>
    {
        public int Id { get; set; }
    }

    public class QueryUserRoleResult
    {
        public int Id { get; set; }
        public int? UserRoleId { get; set; }
        public string RoleName { get; set; }
        public bool Active { get { return this.UserRoleId.HasValue ? true : false; } }
    }

    public class QueryUserPermissionResult
    {
        public int Id { get; set; }
        public int? UserPermissionId { get; set; }
        public int TenantUserId { get; set; }
        public string PermissionName { get; set; }
        public bool Active { get { return this.UserPermissionId.HasValue ? true : false; } }
    }
}