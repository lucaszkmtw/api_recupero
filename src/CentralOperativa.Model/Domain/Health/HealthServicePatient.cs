using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("HealthServicePatients")]
    public class HealthServicePatient
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(HealthService))]
        public int HealthServiceId { get; set; }

        [References(typeof(Patient))]
        public int PatientId { get; set; }

        public string CardNumber { get; set; }

        public string ServicePlan { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string Data1 { get; set; }

        public string Data2 { get; set; }
    }
}