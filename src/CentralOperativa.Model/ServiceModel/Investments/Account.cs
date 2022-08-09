using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/accounts/{Id}", "GET")]
    public class GetAccount : QueryDb<Domain.Investments.Account, QueryAccountsResult>
    {
        public int Id { get; set; }
        

    }

    [Route("/investments/accounts", "POST")]
    [Route("/investments/accounts/{Id}", "PUT")]
    public class PostAccount : Domain.Investments.Account
    {
        public int CurrencyId { get; set; }
        public BusinessPartnerAccountType Type { get; set; }
        public string Prefix { get; set; }

    }

    [Route("/investments/accounts", "GET")]
    public class QueryAccounts : QueryDb<Domain.Investments.Account, QueryAccountsResult>
    {
    }

    [Route("/investments/accounts/lookup", "GET")]
    public class LookupAccounts : LookupRequest, IReturn<List<LookupItem>>
    {
    }


    public class QueryAccountsResult
    {
        public int Id { get; set; }
        //Investor
        public int InvestorId { get; set; }
        public string InvestorPersonName { get; set; }
        public decimal InvestorAssignment { get; set; }
        //Trader
        public int TraderId { get; set; }
        public string TraderPersonName { get; set; }
        public decimal TraderAssignment { get; set; }
        //Manager
        public int ManagerId { get; set; }
        public string ManagerPersonName { get; set; }
        public decimal ManagerAssignment { get; set; }
        //Custodian
        public int CustodianId { get; set; }
        public string CustodianPersonName { get; set; }
        public decimal CustodianAssignment { get; set; }
        //BusinessPartnerAccount
        public string BusinessPartnerAccountName { get; set; }
        public BusinessPartnerAccountType BusinessPartnerAccountType { get; set; }
        public string BusinessPartnerAccountCode { get; set; }
        //Currency
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }


    }

    public class Account : Domain.Investments.Account
    {
        public Account()
        {           
        }

        public Domain.Financials.Currency Currency { get; set;  }
        public BusinessPartnerAccount PartnerAccount { get; set; }
        public decimal Balance { get; set; }
    }
    [Route("/investments/accounts/{Id}", "DELETE")]
    public class DeleteAccount :  Domain.Investments.Account
    {
    }

    public class GetAccountResult : Domain.Investments.Account
    {
    }
}