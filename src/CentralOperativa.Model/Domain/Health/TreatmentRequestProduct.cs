using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("TreatmentRequestProducts")]
    public class TreatmentRequestProduct
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(TreatmentRequest))]
        public int TreatmentRequestId { get; set; }

        [References(typeof(Product))]
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public string Comments { get; set; }

        [References(typeof(Domain.Procurement.Vendor))]
        public int? VendorId { get; set; }

        public decimal? Price { get; set; }
    }
}