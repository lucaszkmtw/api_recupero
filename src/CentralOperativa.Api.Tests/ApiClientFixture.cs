using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using ServiceStack;
using ServiceStack.Auth;

namespace CentralOperativa.Api.Tests
{
    public class ApiClientFixture : IDisposable
    {
        private readonly IWebHost _server;

        public JsonServiceClient ServiceClient { get; }

        public ServiceModel.System.Session Session { get; }

        public ApiClientFixture()
        {
            _server = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5050")
                .Build();
            _server.Start();
            
            ServiceClient = new JsonServiceClient("http://localhost:5050/api");

            //Authenticate
            var authResponse = ServiceClient.Post(new Authenticate
            {
                provider = CredentialsAuthProvider.Name, //= credentials
                UserName = "",
                Password = "",
                RememberMe = true,
            });
            ServiceClient.BearerToken = authResponse.BearerToken;
            Console.WriteLine($"Bearer token: {authResponse.BearerToken}");
            Console.WriteLine($"User: {authResponse.UserId} - {authResponse.UserName}");

            Session = ServiceClient.Get(new ServiceModel.System.GetMySession());
            Console.WriteLine($"TenantId: {Session.TenantId}");
        }
        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
