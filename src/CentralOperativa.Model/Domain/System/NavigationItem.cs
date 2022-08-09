using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Navigation")]
    public class NavigationItem
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(NavigationItem))]
        public int? ParentId { get; set; }

        [References(typeof(Permission))]
        public int PermissionId { get; set; }

        public string State { get; set; }

        public string Name { get; set; }

        public byte ListIndex { get; set; }

        public string IconClass { get; set; }
    }
}