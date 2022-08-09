using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Sales
{
    [Alias("SalesProductCatalog")]
    public class SalesProductCatalog
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}