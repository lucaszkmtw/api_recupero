using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentApprovals")]
    public class BusinessDocumentApproval
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.BusinessDocuments.BusinessDocumentItem))]
        public int DocumentId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        public byte Status { get; set; }

        public DateTime Date { get; set; }
    }
}