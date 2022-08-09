using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("Normatives"), Schema("findm")]
    public class Normative
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Observations { get; set; }
        public int Status { get; set; }
    }
}


