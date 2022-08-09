using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    [Route("/system/roles/lookup", "GET")]
    public class LookupRole : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/system/roles/{Id}", "GET")]
    public class GetRole
    {
        public int Id { get; set; }
    }

    [Route("/system/roles", "GET")]
    public class QueryRoles : QueryDb<Domain.System.Role>
    {
    }

    [Route("/system/roles", "POST")]
    [Route("/system/roles/{Id}", "PUT")]
    public class PostRole : Domain.System.Role
    {
    }

    [Route("/system/roles/{Id}", "DELETE")]
    public class DeleteRole
    {
        public int Id { get; set; }
    }

    [Route("/system/roles/{RoleId}/users/{UserId}", "GET")]
    public class GetRoleUser
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
    }

    [Route("/system/roles/{RoleId}/users", "GET")]
    public class GetUsersByRole : QueryDb<Domain.System.User, QueryRoleUserResult>
        , IJoin<Domain.System.User, Domain.System.UserRole>
        , IJoin<Domain.System.UserRole, Domain.System.Role>
    {
        public int RoleId { get; set; }
        public byte View { get; set; }
    }

    public class QueryRoleUserResult
    {
        public int Id { get; set; }
        public int? UserRoleId { get; set; }
        public string UserName { get; set; }

        public bool Active
        {
            get { return this.UserRoleId.HasValue ? true : false; }
        }
    }

    [Route("/system/roles/{RoleId}/users", "POST")]
    [Route("/system/roles/{RoleId}/users/{UserId}", "PUT")]
    public class PostRoleUser
    {
        public int RoleId { get; set; }
        public int? UserId { get; set; }
        public int[] UserIds { get; set; }
    }

    [Route("/system/roles/{RoleId}/users/delete", "POST")]
    [Route("/system/roles/{RoleId}/users/{UserId}", "DELETE")]
    public class DeleteUserRole
    {
        public int RoleId { get; set; }
        public int? UserId { get; set; }
        public int[] UserIds { get; set; }
    }

    #region RolePermissions

    [Route("/system/roles/{RoleId}/permissions/{PermissionId}", "GET")]
    public class GetRolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }

    [Route("/system/roles/{RoleId}/permissions", "GET")]
    public class GetRolePermissions : QueryDb<Domain.System.Permission, QueryRolePermissionResult>
        , IJoin<Domain.System.Permission, Domain.System.RolePermission>
        , IJoin<Domain.System.RolePermission, Domain.System.Role>
    {
        public int RoleId { get; set; }
        public byte View { get; set; }
    }

    public class QueryRolePermissionResult
    {
        public int Id { get; set; }
        public int? RolePermissionId { get; set; }
        public string PermissionName { get; set; }

        public bool Active
        {
            get { return this.RolePermissionId.HasValue ? true : false; }
        }
    }

    [Route("/system/roles/{RoleId}/permissions", "POST")]
    [Route("/system/roles/{RoleId}/permissions/{PermissionId}", "PUT")]
    public class PostRolePermission
    {
        public int RoleId { get; set; }
        public int? PermissionId { get; set; }
        public int[] PermissionIds { get; set; }
    }

    [Route("/system/roles/{RoleId}/permissions/delete", "POST")]
    [Route("/system/roles/{RoleId}/permissions/{PermissionId}", "DELETE")]
    public class DeleteRolePermission
    {
        public int RoleId { get; set; }
        public int? PermissionId { get; set; }
        public int[] PermissionIds { get; set; }
    }

    #endregion
}
