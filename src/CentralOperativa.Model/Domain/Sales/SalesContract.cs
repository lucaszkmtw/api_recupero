using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Sales
{
    [Alias("SalesContracts")]
    public class SalesContract
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Sales.Client))]
        public int ClientId { get; set; }

        [References(typeof(Catalog.Product))]
        public int ProductId { get; set; }

        [Ignore]
        public string Name { get { return this.ProductId.ToString() + " - " + this.ClientId.ToString(); } }
    }
}