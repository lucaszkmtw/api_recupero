using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.Domain.Factoring
{
    [Alias("Clients"), Schema("factoring")]
    public class Client
    {
        [PrimaryKey]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Capacity { get; set; }

        public decimal Comission { get; set; }

        public decimal Expenses { get; set; }

        public decimal CreditLimit { get; set; }
    }
}
