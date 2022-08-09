using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/bankaccounts/{Id}", "GET")]
    public class GetBankAccount
    {
        public int Id { get; set; }
    }

    public class GetBankAccountResult : Domain.Financials.BankAccount
    {
        public string BankName { get; set; }
    }

    [Route("/financials/bankaccounts", "POST")]
    [Route("/financials/bankaccounts/{Id}", "PUT")]
    public class PostBankAccount : Domain.Financials.BankAccount
    {
    }
    [Route("/financials/bankaccounts", "GET")]
    public class QueryBankAccounts : QueryDb<Domain.Financials.BankAccount, QueryBankAccountResult>
        , IJoin<Domain.Financials.BankAccount, Domain.Financials.BankBranch>
        , IJoin<Domain.Financials.BankAccount, Domain.Financials.Currency>
        , IJoin<Domain.Financials.BankAccount, Domain.System.Persons.Person>
    {
    }

    [Route("/financials/bankaccounts/lookup", "GET")]
    public class LookupBankAccount : LookupRequest, IReturn<List<LookupItem>>
    {
        public int PersonId { get; set; }
    }

    public class QueryBankAccountResult
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public string BankBranchName { get; set; }
        public string CurrencyName { get; set; }
        public string PersonName { get; set; }
    }
}