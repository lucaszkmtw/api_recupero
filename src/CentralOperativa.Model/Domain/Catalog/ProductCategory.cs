using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductCategories"), Schema("catalog")]
    public class ProductCategory
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        public int Status { get; set; }
    }
}