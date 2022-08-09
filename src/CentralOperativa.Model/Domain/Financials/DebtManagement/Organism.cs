using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.BusinessPartners;


namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("Organisms"),Schema("findm")]
    public class Organism
    {
        [PrimaryKey, AutoIncrement]        
        public int Id { get; set; }
        public string Code { get; set; } 
		
		[References(typeof(OrganismType))]  
		public int TypeId { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(BusinessPartner))]
        public int BusinessPartnerId { get; set; }

        public int Status { get; set; }

    }

}