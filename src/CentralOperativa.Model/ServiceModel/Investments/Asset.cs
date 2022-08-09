using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.ServiceModel.Investments
{
    [Route("/investments/assets/{Id}", "GET")]
    public class GetAsset : QueryDb<Domain.Investments.Asset, QueryAssetsResult>
    {
        public int Id { get; set; }


    }

    [Route("/investments/assets", "POST")]
    [Route("/investments/assets/{Id}", "PUT")]
    public class PostAsset : Domain.Investments.Asset
    {
    }

    [Route("/investments/asset", "GET")]
    public class QueryAssets : QueryDb<Domain.Investments.Asset, QueryAssetsResult>
    {
    }

    [Route("/investments/assets/lookup", "GET")]
    public class LookupAssets : LookupRequest, IReturn<List<LookupItem>>
    {
    }


    public class QueryAssetsResult
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
        //Account
        public string AccountId { get; set; }

        //Currency
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }


    }

    public class Asset : Domain.Investments.Asset
    {
        public Asset()
        {
        }
    }
    [Route("/investments/assets/{Id}", "DELETE")]
    public class DeleteAsset : Domain.Investments.Asset
    {
    }

    public class GetAssetResult : Domain.Investments.Asset
    {
    }
}