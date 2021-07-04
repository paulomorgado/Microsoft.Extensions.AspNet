using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc.DependencyInjection
{
    internal sealed class ServiceProviderMvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly System.IServiceProvider serviceProvider;

        public ServiceProviderMvcDependencyResolver(System.IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(System.Type serviceType) => serviceProvider.GetService(serviceType);

        public IEnumerable<object> GetServices(System.Type serviceType) => serviceProvider.GetServices(serviceType);
    }
}
