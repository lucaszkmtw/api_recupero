using ServiceStack;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq;
using CentralOperativa.Domain.System;
using ServiceStack.OrmLite.Dapper;
using Api = CentralOperativa.ServiceModel.System.Navigation;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate]
    public class NavigationServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.GetNavigation request)
        {
            return Db.SingleById<NavigationItem>(request.Id);
        }

        [Authenticate]
        public object Post(Api.NavigationMoveRequest request)
        {
            Db.Execute(string.Format("EXEC MoveNavigation {0}, {1}, {2}", request.Id, request.TargetId, request.Position));
            return true;
        }

        public object Post(Api.PostNavigation request)
        {
            var listIndex = Db.Scalar<NavigationItem, byte>(x => Sql.Max(x.ListIndex), w => w.ParentId == request.ParentId);
            request.ListIndex = listIndex;
            request.Id = (int)Db.Insert((NavigationItem)request, true);
            return request;
        }

        public object Put(Api.PostNavigation request)
        {
            return Db.Update((NavigationItem)request);
        }

        [Authenticate]
        public void Delete(Api.DeleteNavigation request)
        {
            Db.DeleteById<NavigationItem>(request.Id);
        }

        public object Get(Api.QueryNavigation request)
        {
            //var typedSession = session as Session;
            //var userId = typedSession.UserId;
            //var localizationResources = localizationService.Any(new Localization.GetResources { Lang = "en" });

            var navigationItems = Db.Select(Db.From<NavigationItem>());

            var rolePermissions = Db.Select(Db.From<Permission>()
                .Join<Permission, RolePermission>((p, rp) => p.Id == rp.PermissionId)
                .Where<RolePermission>(x => x.RoleId == 1));

            var userPermissions = Db.Select(Db.From<Permission>()
                .Join<Permission, UserPermission>((p, up) => p.Id == up.PermissionId)
                .Where<UserPermission>(x => x.UserId == 1));
            var permissions = rolePermissions.Union(userPermissions).Distinct();
            var permissionIds = permissions.Select(p => p.Id).ToList();

            var menu = navigationItems.Where(x => !x.ParentId.HasValue).OrderBy(x => x.ListIndex)
                .Select(navigationItem => ProcessNavigationItem(navigationItems, navigationItem, permissionIds))
                .Where(menuItem => menuItem != null).ToList();

            return menu;
        }

        private static Api.GetNavigationResult ProcessNavigationItem(List<NavigationItem> allItems, NavigationItem item, List<int> permissionIds)
        {
            var children = allItems.Where(x => x.ParentId.HasValue && x.ParentId == item.Id).ToList();
            if (permissionIds.Contains(item.PermissionId) || children.Any())
            {
                var menuItem = new ServiceModel.System.Navigation.GetNavigationResult
                {
                    Id = item.Id,
                    ListIndex = item.ListIndex,
                    ParentId = item.ParentId,
                    PermissionId = item.PermissionId,
                    State = item.State,
                    Name = item.Name,
                    IconClass = item.IconClass,
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