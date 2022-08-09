using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Alias("BusinessPartnerTypes")]
    public class BusinessPartnerType
    {
        [AutoIncrement]
        public short Id { get; set; }

        [References(typeof(BusinessPartnerType))]
        public short? ParentId { get; set; }

        [References(typeof(Tenant))]
        public int? TenantId { get; set; }

        public string Name { get; set; }
    }
}