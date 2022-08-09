using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    [Route("/localization/resources/reload", "GET")]
    public class GetReloadLocalizationResources : IReturn<GetReloadLocalizationResourcesResponse>
    {
    }

    public class GetReloadLocalizationResourcesResponse
    {
        public bool Success { get; set; }
    }

    [Route("/localization/resources/load", "GET")]
    [Route("/localization/resources/{Lang}", "GET")]
    public class GetLocalizationResources
    {
        public string Lang { get; set; }
    }

    [Route("/localization/resources", "GET")]
    public class QueryLocalizationResources : QueryDb<Domain.System.LocalizationResource>
    {
        public string Name { get; set; }
    }


    [Route("/localization/resources/{Id}")]
    public class GetLocalizationResource
    {
        public int Id { get; set; }
    }


    [Route("/localization/resources", "POST")]
    public class PostLocalizationResource : Domain.System.LocalizationResource
    {
    }

    [Route("/localization/resources/{Id}", "PUT")]
    public class PutLocalizationResource : Domain.System.LocalizationResource
    {
    }

    [Route("/localization/resources/{Id}", "DELETE")]
    public class DeleteLozalizationResource
    {
        public int Id { get; set; }
    }
}