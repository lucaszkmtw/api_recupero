using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/checkbooks/{Id}", "GET")]
    public class GetCheckBook
    {
        public int Id { get; set; }
    }

    public class GetCheckBookResult : Domain.Financials.CheckBook
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountNumber { get; set; }
    }

    [Route("/financials/checkbooks", "POST")]
    [Route("/financials/checkbooks/{Id}", "PUT")]
    public class PostCheckBook : Domain.Financials.CheckBook
    {
    }
    [Route("/financials/checkbooks", "GET")]
    public class QueryCheckBooks : QueryDb<Domain.Financials.CheckBook, QueryCheckBooksResult>
        , IJoin<Domain.Financials.CheckBook, Domain.Financials.BankAccount>
    {
        public int PersonId { get; set; }
    }

    [Route("/financials/checkbooks/lookup", "GET")]
    public class LookupCheckBook : LookupRequest, IReturn<List<LookupItem>>
    {
        public int PersonId { get; set; }
    }

    public class QueryCheckBooksResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
        public string NextNumber { get; set; }
        public string BankAccountCode { get; set; }
    }
}