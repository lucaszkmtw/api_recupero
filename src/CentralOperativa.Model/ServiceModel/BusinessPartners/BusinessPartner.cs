using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Infraestructure;
using ServiceStack;
using System;

namespace CentralOperativa.ServiceModel.BusinessPartners
{
    [Route("/businesspartners/businesspartners/{Id}", "GET")]
    public class GetBusinessPartner
    {
        public int Id { get; set; }
    }

    [Route("/businesspartners/businesspartnersbyperson/{PersonId}", "GET")]
    public class GetBusinessPartnerByPerson
    {
        public int PersonId { get; set; }
        public int TypeId { get; set; }
    }

    public class GetBusinessPartnerResult : BusinessPartner
    {
        public System.Persons.Person Person { get; set; }

        public BusinessPartnerAccounts Accounts { get; set; }

        public GetBusinessPartnerResult()
        {
            Accounts = new BusinessPartnerAccounts();
        }

        public class BusinessPartnerAccounts
        {
            public List<Account> Items { get; set; }

            public List<Currency> Currencies { get; set; }

            public BusinessPartnerAccounts()
            {
                Items = new List<Account>();
                Currencies = new List<Currency>();
            }

            public class Account : BusinessPartnerAccount
            {
                public decimal Balance { get; set; }
            }
        }
    }

    [Route("/businesspartners/businesspartners", "GET")]
    public class QueryBusinessPartners : QueryDb<BusinessPartner, QueryBusinessPartnersResult>
        , IJoin<BusinessPartner, Domain.System.Persons.Person>
    {
        public string[] Types { get; set; }
    }

    public class QueryBusinessPartnersResult
    {
        public int Id { get; set; }
        public short TypeId { get; set; }
        public string Code { get; set; }
        public int PersonId { get; set; }
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
    }

    [Route("/businesspartners/businesspartners", "POST")]
    [Route("/businesspartners/businesspartners/{Id}", "PUT")]
    public class PostBusinessPartner : BusinessPartner
    {
        public System.Persons.Person Person { get; set; }
    }

    [Route("/businesspartners/businesspartners/lookup", "GET")]
    public class LookupBusinessPartner : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/businesspartners/account/{Id}", "GET")]
    public class AccountBusinessPartner : AccountBusinessPartnerResult
    {
    }

    public class AccountBusinessPartnerResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }


    [Route("/businesspartners/account/{AccountId}/entries", "GET")]
    public class AccountBusinessPartnerEntries : QueryDb<BusinessPartnerAccountEntry, AccountBusinessPartnerEntriesResult>
    {
        public int AccountId { get; set; }
    }
    public class AccountBusinessPartnerEntriesResult
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime PostingDate { get; set; }

        public decimal Balance { get; set; }
        public short? LinkedDocumentTypeId { get; set; }

        public int? LinkedDocumentId { get; set; }
    }

    [Route("/businesspartners/businesspartners/persons/{Id}", "GET")]
    public class GetBusinessPartnerPerson : GetBusinessPartnerPersonResult
    {
    }

    public class GetBusinessPartnerPersonResult : BusinessPartner
    {

        public GetBusinessPartnerPersonResult()
        {
            Accounts = new BusinessPartnerAccounts();

            InventorySites = new List<Domain.Inv.InventorySite>();
        }

        public BusinessPartnerAccounts Accounts { get; set; }

        public List<Domain.Inv.InventorySite> InventorySites { get; set; }

        public class BusinessPartnerAccounts
        {
            public List<Account> Items { get; set; }

            public List<Currency> Currencies { get; set; }

            public BusinessPartnerAccounts()
            {
                Items = new List<Account>();
                Currencies = new List<Currency>();
            }

            public class Account : BusinessPartnerAccount
            {
                public decimal Balance { get; set; }
            }
        }
    }
}


//using System.Collections.Generic;
//using CentralOperativa.Domain.BusinessPartners;
//using CentralOperativa.Domain.Financials;
//using CentralOperativa.Infraestructure;
//using ServiceStack;
//using System;

//namespace CentralOperativa.ServiceModel.BusinessPartners
//{
//    [Route("/businesspartners/businesspartners/{Id}", "GET")]
//    public class GetBusinessPartner
//    {
//        public int Id { get; set; }
//    }

//    [Route("/businesspartners/businesspartnersbyperson/{PersonId}", "GET")]
//    public class GetBusinessPartnerByPerson
//    {
//        public int PersonId { get; set; }
//        public int TypeId { get; set; }
//    }

//    public class GetBusinessPartnerResult : BusinessPartner
//    {
//        public System.Persons.Person Person { get; set; }

//        public BusinessPartnerAccounts Accounts { get; set; }

//        public GetBusinessPartnerResult()
//        {
//            this.Accounts = new BusinessPartnerAccounts();
//        }

//        public class BusinessPartnerAccounts
//        {
//            public List<Account> Items { get; set; }

//            public List<Currency> Currencies { get; set; }

//            public BusinessPartnerAccounts()
//            {
//                this.Items = new List<Account>();
//                this.Currencies = new List<Currency>();
//            }

//            public class Account : BusinessPartnerAccount
//            {
//                public decimal Balance { get; set; }
//            }
//        }
//    }

//    [Route("/businesspartners/businesspartners", "GET")]
//    public class QueryBusinessPartners : QueryDb<BusinessPartner, QueryBusinessPartnersResult>
//        , IJoin<BusinessPartner, Domain.System.Persons.Person>
//    {
//        public string[] Types { get; set; }
//    }

//    public class QueryBusinessPartnersResult
//    {
//        public int Id { get; set; }

//        public short TypeId { get; set; }

//        public string Code { get; set; }

//        public int PersonId { get; set; }
//        public string PersonCode { get; set; }
//        public string PersonName { get; set; }
//    }

//    [Route("/businesspartners/businesspartners", "POST")]
//    [Route("/businesspartners/businesspartners/{Id}", "PUT")]
//    public class PostBusinessPartner : BusinessPartner
//    {
//        public System.Persons.PostPerson Person { get; set; }
//    }

//    [Route("/businesspartners/businesspartners/lookup", "GET")]
//    public class LookupBusinessPartner : LookupRequest, IReturn<List<LookupItem>>
//    {
//    }

//    [Route("/businesspartners/account/{Id}", "GET")]
//    public class AccountBusinessPartner : AccountBusinessPartnerResult
//    {
//    }

//    public class AccountBusinessPartnerResult
//    {
//        public int Id { get; set; }

//        public string Name { get; set; }

//        public string Code { get; set; }

//        //public List<BusinessPartnerAccountEntry> BusinessPartnerAccountEntries { get; set; }
//    }


//    [Route("/businesspartners/account/{AccountId}/entries", "GET")]
//    public class AccountBusinessPartnerEntries : QueryDb<BusinessPartnerAccountEntry, AccountBusinessPartnerEntriesResult>
//    {
//        public int AccountId { get; set; }
//    }
//    public class AccountBusinessPartnerEntriesResult
//    {
//        public int Id { get; set; }

//        public int AccountId { get; set; }

//        public decimal Amount { get; set; }

//        public string Code { get; set; }

//        public string Description { get; set; }

//        public DateTime CreateDate { get; set; }

//        public decimal Balance { get; set; }
//        public short? LinkedDocumentTypeId { get; set; }

//        public int? LinkedDocumentId { get; set; }
//    }

//    [Route("/businesspartners/businesspartners/persons/{Id}", "GET")]
//    public class GetBusinessPartnerPerson : GetBusinessPartnerPersonResult
//    {
//    }

//    public class GetBusinessPartnerPersonResult : BusinessPartner
//    {

//        public GetBusinessPartnerPersonResult()
//        {
//            this.Accounts = new BusinessPartnerAccounts();

//            this.InventorySites = new List<Domain.Inv.InventorySite>();
//        }

//        public BusinessPartnerAccounts Accounts { get; set; }

//        public List<Domain.Inv.InventorySite> InventorySites { get; set; }

//        public class BusinessPartnerAccounts
//        {
//            public List<Account> Items { get; set; }

//            public List<Currency> Currencies { get; set; }

//            public BusinessPartnerAccounts()
//            {
//                this.Items = new List<Account>();
//                this.Currencies = new List<Currency>();
//            }

//            public class Account : BusinessPartnerAccount
//            {
//                public decimal Balance { get; set; }
//            }
//        }
//    }
//}
