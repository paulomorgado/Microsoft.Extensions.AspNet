using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class ServiceProviderMvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly System.IServiceProvider serviceProvider;

        public ServiceProviderMvcDependencyResolver(System.IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(System.Type serviceType) => this.serviceProvider.GetService(serviceType);

        public IEnumerable<object> GetServices(System.Type serviceType) => this.serviceProvider.GetServices(serviceType);
    }
}
