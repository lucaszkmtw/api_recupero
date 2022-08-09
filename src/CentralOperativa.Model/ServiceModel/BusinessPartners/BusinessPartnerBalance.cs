using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.BusinessPartners
{
    [Route("/financials/currencies/{Id}", "GET")]
    public class GetBusinessPartnerBalance : IReturn<Domain.BusinessPartners.BusinessPartnerBalance>
    {
        public int AccountId { get; set; }
    }

    public class BusinessPartnerBalance : Domain.BusinessPartners.BusinessPartnerBalance
    {
        public BusinessPartnerBalance()
        {
        }

    }

}