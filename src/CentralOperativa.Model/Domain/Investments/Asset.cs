using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Domain.System;
using System;

namespace CentralOperativa.Domain.Investments
{
    [Alias("Assets"), Schema("investments")]
    public class Asset
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Account))]
        public int AccountId { get; set; }

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

        public decimal Amount { get; set; }
        public decimal ExpirationAmount { get; set; }
        public int Term { get; set; }
        public DateTime ExpirationDate { get; set; }


        public decimal InvestorAssignmentValue { get; set; }
        public decimal ManagerAssignmentValue{ get; set; }
        public decimal CustodianAssignmentValue { get; set; }
        public decimal TraderAssignmentValue { get; set; }
    }
}
