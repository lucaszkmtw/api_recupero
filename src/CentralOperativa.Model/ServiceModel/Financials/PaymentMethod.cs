using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/paymentmethods/{Id}", "GET")]
    public class GetPaymentMethod
    {
        public int Id { get; set; }
    }

    [Route("/financials/paymentmethods", "POST")]
    [Route("/financials/paymentmethods/{Id}", "PUT")]
    public class PostPaymentMethod : Domain.Financials.PaymentMethod
    {
    }


    [Route("/financials/paymentmethods", "GET")]
    public class QueryPaymentMethods : QueryDb<Domain.Financials.PaymentMethod, QueryPaymentMethodsResult>
    {
        public string Description { get; set; }
    }

    public class QueryPaymentMethodsResult : Domain.Financials.PaymentMethod
    {
    }

    [Route("/financials/paymentmethods/lookup", "GET")]
    public class LookupPaymentMethod : LookupRequest, IReturn<List<LookupItem>>
    {
    }
}