using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("Currencies")]
    public class Currency
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}