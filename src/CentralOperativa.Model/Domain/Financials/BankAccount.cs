using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("BankAccounts")]
    public class BankAccount
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        public string Code { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }

        [References(typeof(Domain.Financials.BankBranch))]
        public int BankBranchId { get; set; }

        [References(typeof(Currency))]
        public int CurrencyId { get; set; }
    }
}