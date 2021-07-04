using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.DependencyInjection
{
    internal sealed class WebApiServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IWebObjectActivator webObjectActivator;

        public WebApiServiceProviderDependencyResolver(IWebObjectActivator webObjectActivator)
        {
            this.webObjectActivator = webObjectActivator;
        }

        public IDependencyScope BeginScope() => new ServiceProviderWebApiDependencyScope(webObjectActivator.CreateScope());

        public object GetService(Type serviceType) => webObjectActivator.GetService(serviceType);

        public IEnumerable<object> GetServices(Type serviceType) => webObjectActivator.GetServices(serviceType);

        public void Dispose()
        {
        }

        private sealed class ServiceProviderWebApiDependencyScope : IDependencyScope
        {
            private readonly IServiceScope serviceScope;

            public ServiceProviderWebApiDependencyScope(IServiceScope serviceScope)
            {
                this.serviceScope = serviceScope;
            }

            public object GetService(Type serviceType) => serviceScope.ServiceProvider.GetService(serviceType);

            public IEnumerable<object> GetServices(Type serviceType) => serviceScope.ServiceProvider.GetServices(serviceType);

            public void Dispose()
            {
                serviceScope.Dispose();
            }
        }
    }
}
