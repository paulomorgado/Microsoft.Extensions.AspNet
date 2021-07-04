using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using Microsoft.AspNet.FriendlyUrls;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNet.Hosting.SystemWeb;
using Microsoft.AspNet.Hosting.SystemWeb.Mvc;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SampleWebApplication.Options;
using SampleWebApplication.Services;

namespace SampleWebApplication.App_Start
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FeedOptions>(this.configuration.GetSection("feed"));
            services.Configure<ApplicationOptions>(this.configuration.GetSection("application"));

            services.AddHttpClient<ISyndicationClient, SyndicationClient>()
                .AddPolicyHandler(
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), 5, 2.0, true)));

            services.AddMvc();

            services.AddWebApi();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseWebApi(webApi =>
                {
                    webApi.Configure(config =>
                    {
                        // Web API configuration and services

                        // Web API routes
                        config.MapHttpAttributeRoutes();

                        config.Routes.MapHttpRoute(
                            name: "DefaultApi",
                            routeTemplate: "api/{controller}/{id}",
                            defaults: new { id = RouteParameter.Optional }
                        );
                    });
                });

            app.UseRouting(routes =>
                {
                    routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

                    var settings = new FriendlyUrlSettings();
                    settings.AutoRedirectMode = RedirectMode.Permanent;
                    routes.EnableFriendlyUrls(settings);

                    routes.MapRoute(
                        name: "Default",
                        url: "{controller}/{action}/{id}",
                        defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                    );
                });

            app.UseMvc(mvc =>
                {
                    mvc.RegisterAllAreas();
                    mvc.RegisterGlobalFilters(filters => filters.Add(new HandleErrorAttribute()));
                });

            app.UseBundling(bundles =>
                {
                    bundles.Add(new ScriptBundle("~/bundles/WebFormsJs")
                        .Include(
                            "~/Scripts/WebForms/WebForms.js",
                            "~/Scripts/WebForms/WebUIValidation.js",
                            "~/Scripts/WebForms/MenuStandards.js",
                            "~/Scripts/WebForms/Focus.js",
                            "~/Scripts/WebForms/GridView.js",
                            "~/Scripts/WebForms/DetailsView.js",
                            "~/Scripts/WebForms/TreeView.js",
                            "~/Scripts/WebForms/WebParts.js"));

                    // Order is very important for these files to work, they have explicit dependencies
                    bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs")
                        .Include(
                            "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
                            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
                            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
                            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));

                    bundles.Add(new ScriptBundle("~/bundles/jquery")
                        .Include(
                            "~/Scripts/jquery-{version}.js"));

                    bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                        .Include(
                            "~/Scripts/jquery.validate*"));

                    // Use the Development version of Modernizr to develop with and learn from. Then, when you’re
                    // ready for production, use the build tool at https://modernizr.com to pick only the tests you need
                    bundles.Add(new ScriptBundle("~/bundles/modernizr")
                        .Include(
                            "~/Scripts/modernizr-*"));

                    bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                        .Include(
                            "~/Scripts/bootstrap.js"));

                    bundles.Add(new StyleBundle("~/Content/css")
                        .Include(
                              "~/Content/bootstrap.css",
                              "~/Content/site.css"));
                });
        }
    }
}