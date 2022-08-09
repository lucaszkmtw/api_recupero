using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Catalog;


namespace CentralOperativa.Domain.Financials.DebtManagement
{
    
    [Alias("OrganismProducts"), Schema("findm")]
    
    public class OrganismProduct
    {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }            

            [References(typeof(Product))]
            public int ProductId { get; set; }

            [References(typeof(Organism))]
            public int OrganismId { get; set; }

		    public int Status   { get; set; }
	}
}
