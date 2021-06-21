using Microsoft.Extensions.Hosting.AspNet;
using System;
using System.IO;

namespace Microsoft.Extensions.Configuration.AspNet
{
    public static class AspNetConfigurationConfig
    {
        public static IConfiguration Build(Action<IConfigurationBuilder> configure = null)
        {
            var builder = new ConfigurationBuilder()
                .SetFileProvider(AspNetHostEnvironment.Instance.ContentRootFileProvider)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.{AspNetHostEnvironment.Instance.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configure?.Invoke(builder);

            return builder.Build();
        }
    }
}
