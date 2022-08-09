using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductDocumentTypes"), Schema("catalog")]
    public class ProductDocumentType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Product))]
        public int ProductId { get; set; }

        [References(typeof(BusinessDocuments.BusinessDocumentType))]
        public int DocumentTypeId { get; set; }
    }
}