using System.Collections.Generic;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Health
{
    [Route("/health/healthcareproviders/{Id}", "GET")]
    public class GetHealthCareProvider : IReturn<GetHealthCareProviderResponse>
    {
        public int Id { get; set; }
    }

    public class GetHealthCareProviderResponse : Domain.Health.HealthCareProvider
    {
        public ServiceModel.System.Persons.Person Person { get; set; }
    }

    [Route("/health/healthcareproviders/batch", "POST")]
    public class PostHealthCareProviderBatch
    {
        public List<PostHealthCareProviderBatchItem> Items { get; set; }

        public PostHealthCareProviderBatch()
        {
            this.Items = new List<PostHealthCareProviderBatchItem>();
        }

        public class PostHealthCareProviderBatchItem : Domain.Health.HealthCareProvider
        {
            public System.Persons.PostPerson Person { get; set; }
        }
    }

    [Route("/health/healthcareproviders", "POST")]
    [Route("/health/healthcareproviders/{Id}", "PUT")]
    public class PostHealthCareProvider : Domain.Health.HealthCareProvider
    {
    }

    [Route("/health/healthcareproviders", "GET")]
    public class QueryHealthCareProviders : QueryDb<Domain.Health.HealthCareProvider, QueryHealthCareProvidersResponse>
        , IJoin<Domain.Health.HealthCareProvider, Person>
    {
    }

    [Route("/health/healthcareproviders/lookup", "GET")]
    public class LookupHealthCareProvider : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryHealthCareProvidersResponse
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
