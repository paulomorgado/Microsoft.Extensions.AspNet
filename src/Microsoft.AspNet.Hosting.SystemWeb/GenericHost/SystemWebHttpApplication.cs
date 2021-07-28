using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebHttpApplication : IHttpApplication<SystemWebHttpApplication.Context>
    {
        public SystemWebHttpApplication()
        {
        }

        public Context CreateContext(IFeatureCollection contextFeatures)
        {
            return new Context();
        }

        public Task ProcessRequestAsync(Context context)
        {
            return Task.CompletedTask;
        }

        public void DisposeContext(Context context, Exception exception)
        {
        }

        internal sealed class Context
        {
        }
    }
}