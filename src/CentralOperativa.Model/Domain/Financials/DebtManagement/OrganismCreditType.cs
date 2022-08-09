using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    
    [Alias("OrganismCreditTypes"), Schema("findm")]
    
    public class OrganismCreditType
    {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }            

            [References(typeof(CreditType))]
            public int TypeId { get; set; }

            [References(typeof(Organism))]
            public int OrganismId { get; set; }

		    public int Status   { get; set; }
	}
}
