using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("TreatmentRequestPractices")]
    public class TreatmentRequestPractice
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(TreatmentRequest))]
        public int TreatmentRequestId { get; set; }

        [References(typeof(MedicalPractice))]
        public int MedicalPracticeId { get; set; }

        public decimal Quantity { get; set; }

        public string Frequency { get; set; }

        public string Comments { get; set; }

        [References(typeof(Domain.Procurement.Vendor))]
        public int? VendorId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}