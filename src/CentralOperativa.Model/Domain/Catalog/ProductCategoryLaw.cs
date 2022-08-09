using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductCategoryLaws"), Schema("catalog")]
    public class ProductCategoryLaw
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(ProductCategory))]
        public int ProductCategoryId { get; set; }

        [References(typeof(Domain.Financials.DebtManagement.Law))]
        public int LawId { get; set; }

    }
}