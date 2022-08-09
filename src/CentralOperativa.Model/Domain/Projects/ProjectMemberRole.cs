using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.System;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectMemberRoles"), Schema("projects")]
    public class ProjectMemberRole
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        public string Name { get; set; }
    }
}
