using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("Laws"), Schema("findm")]
    public class Law
    {
       [AutoIncrement]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Prescription { get; set; }
        public int Status { get; set; }
        public DateTime? MaxPrescriptionDate { get; set; }
    }
}
