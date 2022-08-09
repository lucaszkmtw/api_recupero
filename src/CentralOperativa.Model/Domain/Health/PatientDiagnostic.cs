using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("PatientDiagnostics")]
    public class PatientDiagnostic
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Patient))]
        public int PatientId { get; set; }

        [References(typeof(Doctor))]
        public int DoctorId { get; set; }

        [References(typeof(Disease))]
        public int DiseaseId { get; set; }

        [References(typeof(TreatmentRequest))]
        public int TreatmentRequestId { get; set; }

        public DateTime Date { get; set; }

        public string Comments { get; set; }
    }
}