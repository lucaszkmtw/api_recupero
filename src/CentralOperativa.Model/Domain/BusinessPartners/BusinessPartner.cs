using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Alias("BusinessPartners")]
    public class BusinessPartner
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessPartnerType))]
        public short TypeId { get; set; }

        [References(typeof(Domain.System.Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        public string Code { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid Guid { get; set; }

        public BusinessPartnerStatus Status { get; set; }
    }
}