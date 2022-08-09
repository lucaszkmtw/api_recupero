using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("RolePermissions")]
    public class RolePermission
    {
        [AutoIncrement]
        public byte Id { get; set; }

        [References(typeof(Role))]
        public int RoleId { get; set; }

        [References(typeof(Permission))]
        public int PermissionId { get; set; }
    }
}