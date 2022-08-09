using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Sales
{
    [Alias("SalesProductComponents")]
    public class SalesProductComponent
    {
        [AutoIncrement]
        public int Id { get; set; }
        public decimal Quantity { get; set; }

        [References(typeof(Sales.SalesProductCatalog))]
        public int SalesProductId { get; set; }

        [References(typeof(Catalog.Product))]
        public int ProductId { get; set; }
    }
}