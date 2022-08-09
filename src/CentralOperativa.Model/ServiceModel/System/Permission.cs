using CentralOperativa.Infraestructure;
using ServiceStack;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.System
{
    public class Permission
    {
        [Route("/system/permissions/{Id}")]
        public class GetPermission
        {
            public int Id { get; set; }
        }

        [Route("/system/permissions")]
        public class QueryPermissions : QueryDb<Domain.System.Permission>
        {
        }

        [Route("/system/permissions", "POST")]
        [Route("/system/permissions/{Id}", "PUT")]
        public class PostPermission : Domain.System.Permission
        {
        }

        [Route("/system/permissions/user", "POST")]
        [Route("/system/permissions/user/{UserPermissionId}", "PUT")]
        public class PostUserPermission : Domain.System.UserPermission
        {
            public int UserPermissionId { get; set; }
        }

        [Route("/system/permissions/role", "POST")]
        [Route("/system/permissions/role/{RolePermissionId}", "PUT")]
        public class PostRolePermission : Domain.System.RolePermission
        {
            public int RolePermissionId { get; set; }
        }

        [Route("/system/permissions/user/{UserPermissionId}", "GET")]
        public class GetUserPermission : Domain.System.UserPermission
        {
            public int UserPermissionId { get; set; }
        }

        [Route("/system/permissions/role/{RolePermissionId}", "GET")]
        public class GetRolePermission : Domain.System.RolePermission
        {
            public int RolePermissionId { get; set; }
        }

        [Route("/system/permissions/{Id}", "DELETE")]
        public class DeletePermission
        {
            public int Id { get; set; }
        }

        [Route("/system/permissions/{PermissionId}/users", "GET")]
        public class GetUsersByPermission : QueryDb<Domain.System.User, QueryUserPermissionResult>
            , IJoin<Domain.System.User, Domain.System.Persons.Person>
            //, ILeftJoin<Domain.System.User, Domain.System.UserPermission>
        {
            public int PermissionId { get; set; }
        }

        [Route("/system/permissions/{PermissionId}/roles", "GET")]
        public class GetRolesByPermission : QueryDb<Domain.System.Role, QueryRolePermissionResult>
            //, ILeftJoin<Domain.System.Role, Domain.System.RolePermission>
        {
            public int PermissionId { get; set; }
        }

        [Route("/system/permissions/lookup", "GET")]
        public class LookupPermission : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryUserPermissionResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PersonName { get; set; }
            public int? UserPermissionId { get; set; }

            public bool Active
            {
                get { return this.UserPermissionId.HasValue ? true : false; }
            }
        }

        public class QueryRolePermissionResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? RolePermissionId { get; set; }

            public bool Active
            {
                get { return this.RolePermissionId.HasValue ? true : false; }
            }
        }
    }
}