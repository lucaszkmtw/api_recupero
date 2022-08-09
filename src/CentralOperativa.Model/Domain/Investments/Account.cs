using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.System.Persons;
using System;


namespace CentralOperativa.Domain.Investments
{
    [Alias("Accounts"), Schema("investments")]
    public class Account
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessPartnerAccount))]
        public int PartnerAccountId { get; set; }

        [References(typeof(Investor))]
        public int InvestorId { get; set; }

        public decimal InvestorAssignment { get; set; }

        [References(typeof(Manager))]
        public int ManagerId { get; set; }

        public decimal ManagerAssignment { get; set; }

        [References(typeof(Custodian))]
        public int CustodianId { get; set; }

        public decimal CustodianAssignment { get; set; }

        [References(typeof(Trader))]
        public int TraderId { get; set; }

        public decimal TraderAssignment { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
