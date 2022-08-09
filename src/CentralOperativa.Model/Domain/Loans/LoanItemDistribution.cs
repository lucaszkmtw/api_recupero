using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanItemDistributions")]
    public class LoanItemDistribution
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(LoanItem))]
        public int LoanItemId{ get; set; }

        [References(typeof(BusinessPartners.BusinessPartner))]
        public int? BusinessPartnerId { get; set; }

        [Alias("LoanPersonRoleId")]
        public LoanPersonRole? PersonRole { get; set; }

        public decimal Value { get; set; }
    }
}

