using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Auth;

using CentralOperativa.Domain.System;

namespace CentralOperativa.Infraestructure
{
    public class Session : AuthUserSession
    {
        public Session()
        {
            this.Language = "es-ar";

            this.Roles = new List<string>();
            this.Permissions = new List<string>();
        }

        public int TenantId { get; set; }

        public int TenantUserId { get; set; }

        public Guid? ProfileFolderGuid { get; set; }

        public int UserId => string.IsNullOrEmpty(this.UserAuthId) ? 0 : int.Parse(this.UserAuthId);

        /*
        public override bool HasRole(string role)
        {
            if (this.Roles != null)
            {
                return this.Roles.Select(x => x.ToLowerInvariant()).Contains(role.ToLowerInvariant());
            }

            return base.HasRole(role);
        }

        public override bool HasPermission(string permission)
        {
            if (this.Permissions != null)
            {
                return this.Permissions.Select(x => x.ToLowerInvariant()).Contains(permission.ToLowerInvariant());
            }

            return base.HasPermission(permission);
        }
        */

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var service = authService as AuthenticateService;
            var typedSession = session as Session;

            if (service?.Db == null || typedSession == null)
            {
                throw new ApplicationException("Error in AuthenticateService");
            }

            var db = service.Db;

            var userId = typedSession.UserId;
            var tenantUsers = db.Select(db.From<TenantUser>().Where(w => w.UserId == userId).OrderByDescending(o => o.IsDefault));
            var tenantUser = tenantUsers.First();
            typedSession.TenantUserId = tenantUser.Id;
            typedSession.TenantId = tenantUser.TenantId;

            var roles = db.Select(db.From<Role>()
                .Join<Role, UserRole>((r, ur) => r.Id == ur.RoleId)
                .Where<Role>(x => x.TenantId == typedSession.TenantId)
                .And<UserRole>(x => x.UserId == userId));
            session.Roles.AddRange(roles.Select(x => x.Name));

            var permissions = db.Select(db.From<Permission>()
                .Join<Permission, UserPermission>((p, up) => p.Id == up.PermissionId)
                .Where<UserPermission>(x => x.TenantId == typedSession.TenantId && x.UserId == userId));
            session.Permissions.AddRange(permissions.Select(x => x.Name));

            // Agrego también los permissos asociados a los roles que tiene asignados el user
            permissions.AddRange(db.Select(db.From<Permission>()
                .Join<Permission, RolePermission>((p, rp) => p.Id == rp.PermissionId)
                .Join<RolePermission, UserRole>((rp, ur) => rp.RoleId == ur.RoleId)
                .Join<UserRole, Role>()
                .Where<Role>(w => w.TenantId == typedSession.TenantId)
                .And<UserRole>(x => x.UserId == userId)));

            session.Permissions.AddRange(permissions.Select(x => x.Id.ToString()).Distinct());
            base.OnAuthenticated(authService, session, tokens, authInfo);
        }
    }
}
