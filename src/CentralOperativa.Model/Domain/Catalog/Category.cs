using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("Categories"), Schema("catalog")]
    public class Category
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Category))]
        public int? ParentId { get; set; }

        public string Name { get; set; }

    }
}
