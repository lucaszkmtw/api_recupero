using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanPersons")]
    public class LoanPerson
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(Loan))]
        public int LoanId { get; set; }

        [Alias("RoleId")]
        public LoanPersonRole Role { get; set; }
    }
}
