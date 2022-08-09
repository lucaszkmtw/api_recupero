using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ArticleTypes"), Schema("catalog")]
    public class ProductType
    {
        [AutoIncrement]
        public int Id { get; set; }

        //public string Code { get; set; }

        [Alias("descripcion")]
        public string Description { get; set; }
    }
}