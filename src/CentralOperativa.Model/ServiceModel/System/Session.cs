using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    [Route("/sessions/mysession")]
    public class GetMySession : IReturn<Session>
    {
    }

    public class Session
    {
        public Session()
        {
            this.Menu = new List<MenuItem>();
            this.Tenants = new List<GetTenantResponse>();
        }

        public int TenantId { get; set; }

        public List<GetTenantResponse> Tenants { get; set; }

        public List<MenuItem> Menu { get; set; }

        public List<Domain.System.Module> Modules { get; set; }

        public int UserId { get; set; }

        public User Identity { get; set; }

        public Guid ProfileFolderGuid { get; set; }

        public int HelpSectionId { get; set; }
    }

    [Route("/sessions/settenant/{TenantId}", "POST")]
    public class SetTenant : IReturn<SetTenantResponse>
    {
        public int TenantId { get; set; }
    }

    public class SetTenantResponse
    {
        public string BearerToken { get; set; }

        public string RefreshToken { get; set; }

        public Session Session { get; set; }
    }

    [Route("/sessions/impersonate")]
    public class Impersonate
    {
    }
}