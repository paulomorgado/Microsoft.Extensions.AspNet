using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class WebHostOptions
    {
        public WebHostOptions(IConfiguration configuration, string applicationNameFallback)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            ApplicationName = configuration[WebHostDefaults.ApplicationKey] ?? applicationNameFallback;
            StartupAssembly = configuration[WebHostDefaults.StartupAssemblyKey];
            DetailedErrors = configuration.ParseBool(WebHostDefaults.DetailedErrorsKey);
            CaptureStartupErrors = configuration.ParseBool(WebHostDefaults.CaptureStartupErrorsKey);
            Environment = configuration[WebHostDefaults.EnvironmentKey];
            WebRoot = configuration[WebHostDefaults.WebRootKey];
            ContentRootPath = configuration[WebHostDefaults.ContentRootKey];
            SuppressStatusMessages = configuration.ParseBool(WebHostDefaults.SuppressStatusMessagesKey);

            // Search the primary assembly and configured assemblies.
            HostingStartupAssemblies = Split($"{ApplicationName};{configuration[WebHostDefaults.HostingStartupAssembliesKey]}");
            HostingStartupExcludeAssemblies = Split(configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey]);
        }

        public string ApplicationName { get; set; }

        public bool SuppressStatusMessages { get; set; }

        public IReadOnlyList<string> HostingStartupAssemblies { get; set; }

        public IReadOnlyList<string> HostingStartupExcludeAssemblies { get; set; }

        public bool DetailedErrors { get; set; }

        public bool CaptureStartupErrors { get; set; }

        public string Environment { get; set; }

        public string StartupAssembly { get; set; }

        public string WebRoot { get; set; }

        public string ContentRootPath { get; set; }

        public IEnumerable<string> GetFinalHostingStartupAssemblies()
        {
            return HostingStartupAssemblies.Except(HostingStartupExcludeAssemblies, StringComparer.OrdinalIgnoreCase);
        }

        private IReadOnlyList<string> Split(string value)
        {
            return value?.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray()
                ?? Array.Empty<string>();
        }
    }
}
