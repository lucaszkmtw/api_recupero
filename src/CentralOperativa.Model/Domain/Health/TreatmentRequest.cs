using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("TreatmentRequests")]
    public class TreatmentRequest
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.System.Workflows.WorkflowInstance))]
        public int WorkflowInstanceId { get; set; }

        [References(typeof(Domain.Health.Patient))]
        public int PatientId { get; set; }

        [References(typeof(Domain.System.Messages.MessageThread))]
        public int? MessageThreadId { get; set; }

        [References(typeof(Domain.System.DocumentManagement.Folder))]
        public int? FolderId { get; set; }

        public DateTime? Date { get; set; }

        public bool IsUrgent { get; set; }

        [References(typeof(Domain.Health.Pharmacy))]
        public int? PharmacyId { get; set; }
    }
}