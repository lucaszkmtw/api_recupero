using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;


namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("NormativeLaws"), Schema("findm")]
    public class NormativeLaw
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }        

        [References(typeof(Normative))]
        public int NormativeId { get; set; }

        [References(typeof(Law))]
        public int LawId { get; set; }

        public int Status { get; set; }
    }

}

