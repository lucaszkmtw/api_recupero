using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentLinks")]
    public class BusinessDocumentLink
    {
        [References(typeof(Domain.BusinessDocuments.BusinessDocument))]
        public int DocumentId { get; set; }

        [References(typeof(Domain.BusinessDocuments.BusinessDocument))]
        public int LinkedDocumentId { get; set; }

        public int TypeId { get; set; }


    }
}