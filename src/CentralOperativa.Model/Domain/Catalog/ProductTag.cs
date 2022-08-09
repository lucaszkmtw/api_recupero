using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductTags"), Schema("catalog")]
    public class ProductTag
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Product))]
        public int ProductId { get; set; }
    }
}
