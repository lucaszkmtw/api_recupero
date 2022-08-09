using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductCategoryConfig"), Schema("catalog")]
    public class ProductCategoryConfig
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(ProductCategory))]
        public int ProductCategoryId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int ByDefault { get; set; }

        public decimal Value { get; set; }
    }
}