using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;

namespace CentralOperativa
{
    public class Startup
    {
        private readonly IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNodeServices();
            services.AddCors();
            services.AddSingleton(_ => Configuration);

            // Add SignalR
            services.AddSignalRCore().AddRedis();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
                configuration.DisableTelemetry = true;

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                loggerFactory.AddConsole();
            }

            app.UseCors(options => options.WithOrigins("*").AllowAnyMethod().AllowAnyHeader());
            app.UseServiceStack(new AppHost(Configuration as IConfiguration));
        }
    }
}
