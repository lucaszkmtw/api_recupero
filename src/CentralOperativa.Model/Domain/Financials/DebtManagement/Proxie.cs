using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("Proxies"), Schema("findm")]
    public class Proxie
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(Person))]
        public int PersonId { get; set; }

        public int Status { get; set; }

    }

}