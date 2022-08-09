using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Procurement
{
    [Alias("Vendors")]
    public class Vendor
    {
        [PrimaryKey]
        public int Id { get; set; }
    }
}