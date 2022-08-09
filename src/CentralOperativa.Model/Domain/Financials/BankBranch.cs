using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("BankBranches")]
    public class BankBranch
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.Financials.Bank))]
        public int BankId { get; set; }
        public int AddressId { get; set; }

        public string Name { get; set; }
    }
}