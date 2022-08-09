using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Catalog;


namespace CentralOperativa.Domain.Financials.DebtManagement
{

    [Alias("OrganismCategories"), Schema("findm")]

    public class OrganismCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        [References(typeof(Organism))]
        public int OrganismId { get; set; }

        public int Status { get; set; }
    }
}

