using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Notifications
{
    [Route("/system/notifications/emailtemplates", "GET")]
    public class QueryEmailTemplates : QueryDb<Domain.System.Notifications.EmailTemplate, QueryEmailTemplatesResponse>
    {
        public string Q { get; set; }
    }

    public class QueryEmailTemplatesResponse : Domain.System.Notifications.EmailTemplate
    {
    }

    [Route("/system/notifications/emailtemplates/{Id}", "GET")]
    public class GetEmailTemplate : IReturn<GetEmailTemplateResponse>
    {
        public int Id { get; set; }
    }

    public class GetEmailTemplateResponse : Domain.System.Notifications.EmailTemplate
    {
    }


    [Route("/system/notifications/emailtemplates/{Id}", "PUT")]
    [Route("/system/notifications/emailtemplates", "POST")]
    public class PostEmailTemplate : Domain.System.Notifications.EmailTemplate
    {
    }

    [Route("/system/notifications/emailtemplates/lookup", "GET")]
    public class LookupEmailTemplate : LookupRequest
    {
    }
}
