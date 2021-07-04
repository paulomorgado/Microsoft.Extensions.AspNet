using System;
using System.Web;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection
{
    internal sealed class SystemWebServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider serviceProvider;

        public SystemWebServiceProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            return ResolveServiceProvider().GetService(serviceType);
        }

        private IServiceProvider ResolveServiceProvider()
        {
            return HttpContext.Current?.Items[HttpContextKeys.HttpContextServiceProviderKey] as IServiceProvider
                ?? this.serviceProvider;
        }
    }
}
