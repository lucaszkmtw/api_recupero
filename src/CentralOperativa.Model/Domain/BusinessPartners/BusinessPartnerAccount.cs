using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Alias("BusinessPartnerAccounts")]
    public class BusinessPartnerAccount
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("TypeId")]
        public BusinessPartnerAccountType Type { get; set; }

        [References(typeof(Domain.BusinessPartners.BusinessPartner))]
        public int BusinessPartnerId { get; set; }

        [References(typeof(Domain.Financials.Currency))]
        public short CurrencyId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid Guid { get; set; }
    }
}