using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentTypes")]
    public class BusinessDocumentType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Code { get; set; }

        public short? ApprovalWorkflowTypeId { get; set; }

        public int CollectionDocument { get; set; }
    }
}