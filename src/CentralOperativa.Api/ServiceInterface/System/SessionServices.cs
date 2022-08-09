using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CentralOperativa.Domain.System;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate(ApplyTo.Post)]
    public class SessionServices : ApplicationService
    {
        private readonly UserRepository _userRepository;
        private readonly FolderRepository _folderRepository;

        public SessionServices(
            UserRepository userRepository,
            FolderRepository folderRepository)
        {
            _userRepository = userRepository;
            _folderRepository = folderRepository;
        }

        public async Task<Api.Session> Get(Api.GetMySession request)
        {
            var model = new Api.Session();

            if (Session.UserId != 0)
            {
                model.UserId = Session.UserId;
                model.TenantId = Session.TenantId;

                // Menu
                if (Session.Permissions?.Count > 0)
                {
                    var permissionIds = Session.Permissions.Select(x => x.ConvertTo<int>()).ToList();
                    var navigationItems = await Db.SelectAsync(Db.From<NavigationItem>());
                    var menu = navigationItems.Where(x => !x.ParentId.HasValue).OrderBy(x => x.ListIndex)
                        .Select(navigationItem => ProcessNavigationItem(navigationItems, navigationItem, permissionIds))
                        .Where(menuItem => menuItem != null).ToList();
                    model.Menu.AddRange(menu);
                }

                // Tenants
                model.Tenants = await Db.SelectAsync<Api.GetTenantResponse>(Db
                    .From<Tenant>()
                    .Join<TenantUser>()
                    .Join<Domain.System.DocumentManagement.Folder>()
                    .Join<Domain.System.Persons.Person>()
                    .Where<TenantUser>(w => w.UserId == Session.UserId));

                // ProfileFolderGuid
                var user = await _userRepository.GetUser(Db, Session.UserId);
                if (user.FolderId.HasValue)
                {
                    var folder = await _folderRepository.GetFolder(Db, user.FolderId.Value, Session, false);
                    model.ProfileFolderGuid = folder.Guid;
                }

                // HelpSectionId
                var helpSection = (await Db.SelectAsync(Db
                        .From<Domain.Cms.Section>()
                        .Where(w => w.TenantId == Session.TenantId && w.Name == "Help")))
                        .SingleOrDefault();
                if (helpSection != null)
                {
                    model.HelpSectionId = helpSection.Id;
                }

                model.Identity = user;
                return model;
            }

            throw new HttpError(HttpStatusCode.Unauthorized, "Session is not authenticated.");
        }

        public async Task<Api.SetTenantResponse> Post(Api.SetTenant request)
        {
            Session.TenantId = request.TenantId;

            Session.Roles.Clear();
            var roles = Db.Select(Db.From<Role>()
                .Join<Role, UserRole>((r, ur) => r.Id == ur.RoleId)
                .Where<Role>(x => x.TenantId == Session.TenantId)
                .And<UserRole>(x => x.UserId == Session.UserId));
            Session.Roles.AddRange(roles.Select(x => x.Name));

            Session.Permissions.Clear();
            var permissions = Db.Select(Db.From<Permission>()
                .Join<Permission, UserPermission>((p, up) => p.Id == up.PermissionId)
                .Where<UserPermission>(x => x.TenantId == Session.TenantId && x.UserId == Session.UserId));
            Session.Permissions.AddRange(permissions.Select(x => x.Name));

            // Agrego también los permissos asociados a los roles que tiene asignados el user
            permissions.AddRange(Db.Select(Db.From<Permission>()
                .Join<Permission, RolePermission>((p, rp) => p.Id == rp.PermissionId)
                .Join<RolePermission, UserRole>((rp, ur) => rp.RoleId == ur.RoleId)
                .Join<UserRole, Role>()
                .Where<Role>(w => w.TenantId == Session.TenantId)
                .And<UserRole>(x => x.UserId == Session.UserId)));
            Session.Permissions.AddRange(permissions.Select(x => x.Id.ToString()).Distinct());

            this.SaveSession(Session);

            string bearerToken = null;
            string refreshToken = null;

            var jwtAuthProvider = AuthenticateService.GetAuthProvider(JwtAuthProviderReader.Name) as JwtAuthProvider;
            if (jwtAuthProvider != null)
            {
                bearerToken = jwtAuthProvider.CreateJwtBearerToken(Session, Session.Roles, Session.Permissions);
                refreshToken = jwtAuthProvider.CreateJwtRefreshToken(Session.UserAuthId, TimeSpan.FromHours(3));
            }

            var session = await Get(new Api.GetMySession());
            var model = new Api.SetTenantResponse
            {
                BearerToken = bearerToken,
                RefreshToken = refreshToken,
                Session = session
            };
            return model;
        }

        public object Post(Api.Impersonate request)
        {
            return null;
        }

        private static MenuItem ProcessNavigationItem(List<NavigationItem> allItems, NavigationItem item, List<int> permissionIds)
        {
            var children = allItems.Where(x => x.ParentId.HasValue && x.ParentId == item.Id).ToList();
            if (permissionIds.Contains(item.PermissionId) || children.Any())
            {
                var menuItem = new MenuItem
                {
                    Id = item.Id,
                    ListIndex = item.ListIndex,
                    ParentId = item.ParentId,
                    State = item.State,
                    Text = item.Name,
                    IconClass = item.IconClass
                };

                foreach (var child in children.OrderBy(x => x.ListIndex))
                {
                    var childMenuItem = ProcessNavigationItem(allItems, child, permissionIds);
                    if (childMenuItem != null)
                    {
                        menuItem.Items.Add(childMenuItem);
                    }
                }

                return (menuItem.Items.Count > 0 || permissionIds.Contains(item.PermissionId)) ? menuItem : null;
            }

            return null;
        }
    }
}
