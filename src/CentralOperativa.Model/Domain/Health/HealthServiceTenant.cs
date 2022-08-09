using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("HealthServiceTenants")]
    public class HealthServiceTenant
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(HealthService))]
        public int HealthServiceId { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        [References(typeof(System.User))]
        public int CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}