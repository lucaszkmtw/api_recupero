using ServiceStack;
using ServiceStack.Auth;
using System.Collections.Generic;
using ServiceStack.Web;
using ServiceStack.Configuration;

namespace CentralOperativa.Infraestructure
{
    public class CustomCredentialsAuthProvider : CredentialsAuthProvider
    {
        private readonly IAppSettings _appSettings;

        public CustomCredentialsAuthProvider(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public override IHttpResult OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            if (session is Session customSession)
            {
                if (RequestContext.Instance.Items.Contains("TenantUserId"))
                {
                    customSession.TenantUserId = int.Parse(RequestContext.Instance.Items["TenantUserId"].ToString());
                }
                authService.SaveSession(customSession, SessionExpiry);
            }
            return base.OnAuthenticated(authService, session, tokens, authInfo);
        }
    }
}