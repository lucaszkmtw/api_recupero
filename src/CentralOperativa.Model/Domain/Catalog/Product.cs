using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.System;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("Products"), Schema("catalog")]
    public class Product
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        public string Name { get; set; }

        public bool Sale { get; set; }

        public bool Purchase { get; set; }
    }
}