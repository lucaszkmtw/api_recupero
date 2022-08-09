using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("Banks")]
    public class Bank
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
    }
}