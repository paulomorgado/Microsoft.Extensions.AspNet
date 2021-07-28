using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    public class SystemWebServerSetupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var server = app.ApplicationServices.GetService<IServer>();
                if (server?.GetType() != typeof(SystemWebServer))
                {
                    throw new InvalidOperationException("Application is running inside System.Web process but is not configured to use System.Web server.");
                }

                next(app);
            };
        }
    }
}
