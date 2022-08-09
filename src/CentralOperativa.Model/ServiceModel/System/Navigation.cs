using System.Collections.Generic;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    public class Navigation
    {
        [Route("/system/navigation/{Id}", "GET")]
        public class GetNavigation
        {
            public int Id { get; set; }
        }

        [Route("/system/navigation", "GET")]
        public class QueryNavigation
        {
            public int? Id { get; set; }
        }

        public class GetNavigationResult
        {
            public GetNavigationResult()
            {
                this.Items = new List<GetNavigationResult>();
            }

            public int Id { get; set; }

            public int? ParentId { get; set; }

            public int PermissionId { get; set; }

            public string Name { get; set; }

            public string IconClass { get; set; }

            public string State { get; set; }

            public int ListIndex { get; set; }

            public List<GetNavigationResult> Items { get; set; }
        }

        [Route("/system/navigation/{Id}/move")]
        public class NavigationMoveRequest
        {
            public int Id { get; set; }
            public int TargetId { get; set; }
            public byte Position { get; set; }
        }

        [Route("/system/navigation/{Id}", "PUT")]
        [Route("/system/navigation", "POST")]
        public class PostNavigation : Domain.System.NavigationItem
        {
        }

        [Route("/system/navigation/{Id}", "DELETE")]
        public class DeleteNavigation : IReturnVoid
        {
            public int Id { get; set; }
        }
    }
}
