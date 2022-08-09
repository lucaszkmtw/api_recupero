using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.BusinessDocuments.BusinessDocumentType
{
    [Route("/businessdocuments/types/lookup", "GET")]
    public class LookupBusinessDocumentTypeRequest : LookupRequest
    {
    }

    [Route("/businessdocuments/types", "GET")]
    public class QueryBusinessDocumentTypes : QueryDb<Domain.BusinessDocuments.BusinessDocumentType, QueryBusinessDocumentTypeResult>
    {
        public int Module { get; set; }
        public int CollectionDocument { get; set; }
    }

    public class QueryBusinessDocumentTypeResult : Domain.BusinessDocuments.BusinessDocumentType
    {        
    }

    [Route("/businessdocuments/types/{Id}", "GET")]
    public class GetBusinessDocumentTypeRequest
    {
        public int Id { get; set; }
    }

    [Route("/businessdocuments/types/params/{Code}", "GET")]
    public class GetBusinessDocumentTypeParamsRequest
    {
        public string Code { get; set; }
    }

    [Route("/businessdocuments/types", "POST")]
    [Route("/businessdocuments/types/{Id}", "POST, PUT")]
    public class PostBusinessDocumentTypeRequest : Domain.BusinessDocuments.BusinessDocumentType
    {
    }

    [Route("/businessdocuments/types/{Id}", "DELETE")]
    public class DeleteBusinessDocumentTypeRequest : IReturnVoid
    {
        public int Id { get; set; }
    }
}