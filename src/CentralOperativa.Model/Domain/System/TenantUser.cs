using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("TenantUsers")]
    public class TenantUser
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsDefault { get; set; }

        public string InitialState { get; set; }
    }
}