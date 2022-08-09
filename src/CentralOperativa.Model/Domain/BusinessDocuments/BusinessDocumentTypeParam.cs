using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentTypeParams")]
    public class BusinessDocumentTypeParam
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessDocumentType))]
        public int TypeId { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        [References(typeof(System.Notifications.EmailTemplate))]
        public int PrintingTemplateId { get; set; }

        public decimal InterestRate { get; set; }
    }
}