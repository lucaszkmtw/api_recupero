using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("TenantModules")]
    public class TenantModule
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Module))]
        public int ModuleId { get; set; }

        [References(typeof(User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }
    }
}