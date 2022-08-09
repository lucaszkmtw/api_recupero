using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/paymentdocuments/types/lookup", "GET")]
    public class LookupPaymentDocumentTypeRequest : LookupRequest
    {
    }

    [Route("/financials/paymentdocuments/types", "GET")]
    public class QueryPaymentDocumentTypes : QueryDb<Domain.Financials.PaymentDocumentType, QueryPaymentDocumentTypeResult>
    {
    }

    public class QueryPaymentDocumentTypeResult : Domain.Financials.PaymentDocumentType
    {
    }

    [Route("/financials/paymentdocuments/types/{Id}", "GET")]
    public class GetPaymentDocumentTypeRequest
    {
        public int Id { get; set; }
    }

    [Route("/financials/paymentdocuments/types", "POST")]
    [Route("/financials/paymentdocuments/types/{Id}", "POST, PUT")]
    public class PostPaymentDocumentTypeRequest : Domain.Financials.PaymentDocumentType
    {
    }

    [Route("/financials/paymentdocuments/{Id}", "DELETE")]
    public class DeletePaymentDocumentTypeRequest : IReturnVoid
    {
        public int Id { get; set; }
    }
}