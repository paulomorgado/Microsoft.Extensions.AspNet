using System.Collections.Generic;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc.DependencyInjection
{
    internal sealed class ServiceProviderMvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly IWebObjectActivator webObjectActivator;

        public ServiceProviderMvcDependencyResolver(IWebObjectActivator webObjectActivator)
        {
            this.webObjectActivator = webObjectActivator;
        }

        public object GetService(System.Type serviceType) => webObjectActivator.GetService(serviceType);

        public IEnumerable<object> GetServices(System.Type serviceType) => webObjectActivator.GetServices(serviceType);
    }
}
