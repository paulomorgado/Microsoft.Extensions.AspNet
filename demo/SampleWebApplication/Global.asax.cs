using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AspNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.AspNet;
using Microsoft.Extensions.DependencyInjection.AspNetMvc;
using Microsoft.Extensions.DependencyInjection.AspNetWebApi;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SampleWebApplication.Options;
using SampleWebApplication.Services;
using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SampleWebApplication
{
    public class Global : HttpApplication
    {
        public Global()
        {
            AspNetDependencyInjectionConfig.RegisterHttpApplication(this);
        }

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Add dependency injection
            AspNetDependencyInjectionConfig.RegisterServiceProvider(services =>
            {
                services.AddSingleton<IConfigureOptions<FeedOptions>>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    return new ConfigureOptions<FeedOptions>(options => configuration.GetSection("feed").Bind(options));
                });

                services.AddSingleton<IConfigureOptions<ApplicationOptions>>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    return new ConfigureOptions<ApplicationOptions>(options => configuration.GetSection("application").Bind(options));
                });

                services.AddHttpClient<ISyndicationClient, SyndicationClient>()
                    .AddPolicyHandler(
                        HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), 5, 2.0, true)));
            })
                .SetMvcDependencyResolver()
                .SetWebApiDependencyResolver();
        }
    }
}