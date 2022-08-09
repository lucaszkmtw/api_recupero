using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("TreatmentRequestDrugs")]
    public class TreatmentRequestDrug
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(TreatmentRequest))]
        public int TreatmentRequestId { get; set; }

        [References(typeof(Drug))]
        public int DrugId { get; set; }

        [References(typeof(CommercialDrug))]
        public int? CommercialDrugId { get; set; }

        public decimal Quantity { get; set; }

        public string Frequency { get; set; }

        public string Comments { get; set; }

        [References(typeof(Domain.Procurement.Vendor))]
        public int? VendorId { get; set; }

        public decimal? Price { get; set; }
    }
}