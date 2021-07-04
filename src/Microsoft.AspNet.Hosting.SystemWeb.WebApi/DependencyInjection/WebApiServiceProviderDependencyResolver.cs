using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.DependencyInjection
{
    internal sealed class WebApiServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;

        public WebApiServiceProviderDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDependencyScope BeginScope() => new ServiceProviderWebApiDependencyScope(serviceProvider.CreateScope());

        public object GetService(Type serviceType) => serviceProvider.GetService(serviceType);

        public IEnumerable<object> GetServices(Type serviceType) => serviceProvider.GetServices(serviceType);

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
