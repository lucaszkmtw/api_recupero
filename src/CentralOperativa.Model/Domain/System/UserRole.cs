using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("UserRoles")]
    public class UserRole
    {
        [AutoIncrement]
        public byte Id { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Role))]
        public int RoleId { get; set; }
    }
}