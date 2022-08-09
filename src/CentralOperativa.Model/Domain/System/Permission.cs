using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Permissions")]
    public class Permission
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}