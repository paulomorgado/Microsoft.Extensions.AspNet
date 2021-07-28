using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebHostModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            SystemWebHosting.GetHost().Services.GetRequiredService<IPipelineManager>().Register(httpApplication);
        }

        public void Dispose()
        {
        }
    }
}
