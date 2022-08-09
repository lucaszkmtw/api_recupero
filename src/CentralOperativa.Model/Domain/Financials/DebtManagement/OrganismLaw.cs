using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;


namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("OrganismLaws"), Schema("findm")]
    public class OrganismLaw
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Normative))]
        public int OrganismId { get; set; }

        [References(typeof(Law))]
        public int LawId { get; set; }

        public int Status { get; set; }
    }

}
