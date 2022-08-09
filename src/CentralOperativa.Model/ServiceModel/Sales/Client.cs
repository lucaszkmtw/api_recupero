using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Sales
{
    [Route("/sales/clients/{Id}", "GET")]
    public class GetClient
    {
        public int Id { get; set; }
    }

    public class GetClientResult : Domain.Sales.Client
    {
        public ServiceModel.BusinessPartners.GetBusinessPartnerResult BusinessPartner { get; set; }
    }

    [Route("/sales/clients", "GET")]
    public class QueryClients : QueryDb<Domain.Sales.Client, QueryClientsResult>
        , IJoin<Domain.Sales.Client, Domain.System.Persons.Person>
    {
    }

    [Route("/sales/clients", "POST")]
    [Route("/sales/clients/{Id}", "PUT")]
    public class PostClient : Domain.Sales.Client
    {
        public ServiceModel.BusinessPartners.PostBusinessPartner BusinessPartner { get; set; }
    }

    [Route("/sales/clients/{Id}", "DELETE")]
    public class DeleteClient
    {
        public int Id { get; set; }
    }

    [Route("/sales/clients/lookup", "GET")]
    public class LookupClient : LookupRequest, IReturn<List<LookupItem>>
    {
        public bool? ReturnPersonId { get; set; }
    }

    public class QueryClientsResult
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
    }
}
