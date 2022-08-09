using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Funq;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.Redis;
using ServiceStack.Text;
using ServiceStack.Validation;
using ServiceStack.Web;

using CentralOperativa.Infraestructure;
using ServiceStack.Azure.Storage;
using ServiceStack.IO;
using ServiceStack.VirtualPath;

namespace CentralOperativa
{
    public class AppHost : AppHostBase
    {
        private readonly IConfiguration _configuration;
        public RSAParameters? JwtRsaPrivateKey;
        public RSAParameters? JwtRsaPublicKey;
        public bool JwtEncryptPayload = false;

        //Tell Service Stack the name of your application and where to find your web services
        public AppHost(IConfiguration configuration) : base("Central Operativa Api", typeof(AppHost).Assembly)
        {
            _configuration = configuration;
        }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new DebugLogFactory();

            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            //Set JSON web services to return idiomatic JSON camelCase properties
            JsConfig.IncludeNullValues = true;
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.AlwaysUseUtc = true;

            this.Config.AllowSessionCookies = false;
            //this.Config.AddRedirectParamsToQueryString = true;
            //this.Config.ApiVersion = "v1";
            this.Config.HandlerFactoryPath = "api";
            this.Config.GlobalResponseHeaders.Remove("X-Powered-By");
            this.Config.GlobalResponseHeaders.Remove("Server");

            // Cache
            var cacheManagerSetting = _configuration["CacheManager"];
            if (cacheManagerSetting.ToLowerInvariant() == "redis")
            {
                var redisConnectionString = _configuration.GetConnectionString("Redis");
                var redisClientManager = new BasicRedisClientManager(redisConnectionString);
                container.Register<IRedisClientsManager>(c => redisClientManager);
                container.Register(c => c.Resolve<IRedisClientsManager>().GetCacheClient()).ReusedWithin(Funq.ReuseScope.None);
            }
            else
            {
                container.Register<ICacheClient>(new MemoryCacheClient());
            }

            container.Register<ISessionFactory>(c => new SessionFactory(c.Resolve<ICacheClient>()));

            // SQL RLS
            GlobalRequestFilters.Add((req, res, dto) =>
            {
                var sessionObj = req.GetSession();
                var session = sessionObj as Session;
                if (session.TenantUserId != 0 && !RequestContext.Instance.Items.Contains("TenantUserId"))
                {
                    RequestContext.Instance.Items.Add("TenantUserId", session.TenantUserId);
                    RequestContext.Instance.Items.Add("TenantId", session.TenantId);
                    RequestContext.Instance.Items.Add("UserId", session.UserId);
                }
            });

            //ORM
            OrmLiteConfig.IsCaseInsensitive = true;
            OrmLiteConfig.StripUpperInLike = true;
            var dialectProvider = new SqlServer2012OrmLiteDialectProvider();
            dialectProvider.GetDateTimeConverter().DateStyle = DateTimeKind.Utc;

            var connectionString = _configuration.GetConnectionString("SQL");
            var factory = new OrmLiteConnectionFactory(connectionString, SqlServer2017Dialect.Provider)
            {
                DialectProvider = { StringSerializer = new JsonStringSerializer() },
                ConnectionFilter = (db =>
                {
                    if (!RequestContext.Instance.Items.Contains("HasSessionContext") && RequestContext.Instance.Items.Contains("TenantUserId"))
                    {
                        if (int.TryParse(RequestContext.Instance.Items["TenantUserId"].ToString(), out var tenantUserId))
                        {
                            if (tenantUserId != 0)
                            {
                                db.SetSessionContextValue("TenantUserId", tenantUserId);
                                db.SetSessionContextValue("TenantId", int.Parse(RequestContext.Instance.Items["TenantId"].ToString()));
                                db.SetSessionContextValue("UserId", int.Parse(RequestContext.Instance.Items["UserId"].ToString()));
                                RequestContext.Instance.Items.Add("HasSessionContext", true);
                            }
                        }
                    }

                    return db;
                }),
                AutoDisposeConnection = true
            };
            container.Register<IDbConnectionFactory>(factory);

            var corsPlugin = new CorsFeature { AutoHandleOptionsRequests = true };
            Plugins.Add(corsPlugin);
            Plugins.Add(new AutoQueryFeature{ IncludeTotal = true });
            Plugins.Add(new OpenApiFeature());
            // Plugins.Add(new ValidationFeature());
            ConfigureAuth(container);

            container.RegisterAutoWired<ServiceInterface.System.TenantRepository>();

            container.RegisterAutoWired<ServiceInterface.System.Persons.PersonRepository>();
            container.RegisterAutoWired<ServiceInterface.System.UserRepository>();

            container.RegisterAutoWired<ServiceInterface.System.Messages.MessageRepository>();

            container.RegisterAutoWired<ServiceInterface.BusinessPartners.BusinessPartnerRepository>();
            container.RegisterAutoWired<ServiceInterface.BusinessPartners.BusinessPartnerAccountRepository>();

            container.RegisterAutoWired<ServiceInterface.System.Workflows.WorkflowActivityRepository>();
            container.RegisterAutoWired<ServiceInterface.System.Workflows.WorkflowInstanceRepository>();
            container.RegisterAutoWired<ServiceInterface.System.DocumentManagement.FolderRepository>();
            container.RegisterAutoWired<ServiceInterface.System.DocumentManagement.FileRepository>();

            container.RegisterAutoWired<ServiceInterface.Cms.Forms.FormRepository>();

            container.RegisterAutoWired<ServiceInterface.Financials.PaymentDocumentRepository>();

            container.RegisterAutoWired<ServiceInterface.Financials.Controlling.CostCenterRepository>();

            container.RegisterAutoWired<ServiceInterface.Investments.AssetRepository>();

            container.RegisterAutoWired<ServiceInterface.Projects.ProjectRepository>();
        }

        private void ConfigureAuth(Container container)
        {
            container.Register<IUserAuthRepository>(c => new UserAuthRepository(c.Resolve<IDbConnectionFactory>()));
            container.Register<IAuthRepository>(c => c.Resolve<IUserAuthRepository>());

            var key = AesUtils.CreateKey();
            var keyString = Convert.ToBase64String(key);
            //var keyString = Convert.FromBase64CharArray(key);

            //Register all Authentication methods you want to enable for this web app.            
            this.Plugins.Add(new AuthFeature(
                () => new Session(), //Use your own typed Custom UserSession type
                new IAuthProvider[] {
                    new BasicAuthProvider(AppSettings),
                    /*
                    new ApiKeyAuthProvider(AppSettings) {
                        RequireSecureConnection = false,
                        PersistSession = true,
                        SessionCacheDuration = TimeSpan.FromMinutes(10)
                    },
                    */
                    new JwtAuthProvider(AppSettings)
                    {
                        AuthKey = JwtRsaPrivateKey != null || JwtRsaPublicKey != null ? null : key,
                        SetBearerTokenOnAuthenticateResponse = true,
                        RequireSecureConnection = false,
                        HashAlgorithm = JwtRsaPrivateKey != null || JwtRsaPublicKey != null ? "RS256" : "HS256",
                        PublicKey = JwtRsaPublicKey,
                        PrivateKey = JwtRsaPrivateKey,
                        EncryptPayload = JwtEncryptPayload,
                        CreatePayloadFilter = CreatePayloadFilter,
                        PopulateSessionFilter = PopulateSessionFilter,
                        PersistSession = true
                    },
                    new CustomCredentialsAuthProvider(AppSettings)
                }));
            
            //Provide service for new users to register so they can login with supplied credentials.
            //Plugins.Add(new RegistrationFeature());

            //override the default registration validation with your own custom implementation
            //container.RegisterAs<CustomRegistrationValidator, IValidator<Registration>>();
        }

        private void CreatePayloadFilter(JsonObject payload, IAuthSession session)
        {
            var typedSession = session as Session;
            payload["tenantId"] = typedSession.TenantId.ToString();
        }

        private void PopulateSessionFilter(IAuthSession session, JsonObject payload, IRequest request)
        {
            var typedSession = session as Session;
            // for some reason by default this reader loses the auth0| prefix from the user's id when it populates
            // the userauthid, so we'll override that behaviour here
            typedSession.TenantId = int.Parse(payload["tenantId"]);
        }

        /*
        public override List<IVirtualPathProvider> GetVirtualFileSources()
        {
            var existingProviders = base.GetVirtualFileSources();
            var connectionString = _configuration.GetConnectionString("Storage");
            existingProviders.Add(new AzureBlobVirtualFiles(connectionString, "centraloperativa-files"));
            return existingProviders;
        }
        */
    }
}
