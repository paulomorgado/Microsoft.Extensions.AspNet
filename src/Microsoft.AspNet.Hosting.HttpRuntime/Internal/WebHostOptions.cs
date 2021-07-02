using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal class WebHostOptions
    {
        public WebHostOptions(IConfiguration configuration, string applicationNameFallback)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            ApplicationName = configuration[HttpRuntimeWebHostDefaults.ApplicationKey] ?? applicationNameFallback;
            StartupAssembly = configuration[HttpRuntimeWebHostDefaults.StartupAssemblyKey];
            DetailedErrors = configuration.ParseBool(HttpRuntimeWebHostDefaults.DetailedErrorsKey);
            CaptureStartupErrors = configuration.ParseBool(HttpRuntimeWebHostDefaults.CaptureStartupErrorsKey);
            Environment = configuration[HttpRuntimeWebHostDefaults.EnvironmentKey];
            WebRoot = configuration[HttpRuntimeWebHostDefaults.WebRootKey];
            ContentRootPath = configuration[HttpRuntimeWebHostDefaults.ContentRootKey];
            SuppressStatusMessages = configuration.ParseBool(HttpRuntimeWebHostDefaults.SuppressStatusMessagesKey);
        }

        public string ApplicationName { get; set; }

        public bool SuppressStatusMessages { get; set; }

        public bool DetailedErrors { get; set; }

        public bool CaptureStartupErrors { get; set; }

        public string Environment { get; set; }

        public string StartupAssembly { get; set; }

        public string WebRoot { get; set; }

        public string ContentRootPath { get; set; }
    }
}
