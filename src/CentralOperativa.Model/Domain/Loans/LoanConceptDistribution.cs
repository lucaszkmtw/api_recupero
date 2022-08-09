using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanConceptDistributions")]
    public class LoanConceptDistribution
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId { get; set; }

        [References(typeof(LoanConcept))]
        public int LoanConceptId{ get; set; }

        [References(typeof(BusinessPartners.BusinessPartner))]
        public int? BusinessPartnerId { get; set; }

        [Alias("LoanPersonRoleId")]
        public LoanPersonRole? PersonRole { get; set; }

        public decimal Percentage { get; set; }
    }
}

