using System;
using ServiceStack.DataAnnotations;


namespace CentralOperativa.Domain.Financials
{
    [Alias("InvestmentAccounts")]
    public class InvestmentAccount
    {
        [AutoIncrement]
        public int Id { get; set; }

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

        public string Alias { get; set; }
    }
}
