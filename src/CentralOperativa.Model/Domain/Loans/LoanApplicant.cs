using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    public class LoanApplicant
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }
    }
}
