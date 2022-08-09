using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Sales
{
    [Alias("PaymentConditions")]
    public class PaymentCondition
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        [References(typeof(Sales.SalesProductCatalog))]
        public int ProductId { get; set; }
    }
}