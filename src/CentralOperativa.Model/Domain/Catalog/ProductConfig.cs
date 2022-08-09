using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductConfig"), Schema("catalog")]
    public class ProductConfig
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Product))]
        public int ProductId { get; set; }

        public string FieldsJSON { get; set; }
    }
}
