using System;
using System.Reflection;
using System.Web.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.Hosting.AspNet
{
    public static class AspNetHostEnvironment
    {
        public static readonly IHostEnvironment Instance = new HostEnvironment();

        private sealed class HostEnvironment : IHostEnvironment
        {
            public HostEnvironment()
            {
                this.EnvironmentName = WebConfigurationManager.AppSettings["Microsoft:AspNet:Hosting:HttpRuntime:Environment"];
                if (string.IsNullOrWhiteSpace(this.EnvironmentName))
                {
                    this.EnvironmentName = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");
                }
                if (string.IsNullOrWhiteSpace(this.EnvironmentName))
                {
                    this.EnvironmentName = Environments.Production;
                }

                this.ApplicationName = Assembly.GetEntryAssembly()?.GetName()?.Name;

                this.ContentRootPath = System.Web.HttpRuntime.AppDomainAppPath;

                this.ContentRootFileProvider = new PhysicalFileProvider(this.ContentRootPath);
            }

            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}
