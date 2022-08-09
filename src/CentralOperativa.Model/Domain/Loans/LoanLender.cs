using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanLenders")]
    public class LoanLender
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
