using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Infraestructure;
using ServiceStack;
using System;

namespace CentralOperativa.ServiceModel.BusinessPartners
{
    [Route("/businesspartnertypes/{Id}", "GET")]

    public class GetBusinessPartnerType
    {
        public int Id { get; set; }
		public string Name { get; set; }
	}

	
	[Route("/businesspartnertypes", "GET")]
	public class QueryBusinessPartnerTypes
	{
		public string Name { get; set; }
	}

	[Route("/businesspartnertypes/lookup", "GET")]
    public class LookupBusinessPartnerType : LookupRequest, IReturn<List<LookupItem>>
    {
    }

}
