using System;
using ServiceStack.DataAnnotations;


namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("Prescriptions"), Schema("findm")]
    public class Prescription
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Normative))]
        public int NormativeId { get; set; }

        public int NumberOfDays { get; set; }
        public string Observations { get; set; }
        public string Suspends { get; set; }
        public string Interrupt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
    }
}