using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonReferences")]
    public class Reference
    {
        [References(typeof(Person))]
        public int? PersonId { get; set; }

        [References(typeof(Health.Doctor))]
        public int? DoctorId { get; set; }

        [References(typeof(Health.HealthService))]
        public int? HealthServiceId { get; set; }

        [References(typeof(Health.Patient))]
        public int? PatientId { get; set; }

        //[References(typeof(Procurement.Vendor))]
        //public int? VendorId { get; set; }
    }
}