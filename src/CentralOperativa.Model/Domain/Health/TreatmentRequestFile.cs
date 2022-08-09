using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("TreatmentRequestFiles")]
    public class TreatmentRequestFile
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(TreatmentRequest))]
        public int TreatmentRequestId { get; set; }

        [References(typeof(System.DocumentManagement.File))]
        public int FileId { get; set; }
    }
}