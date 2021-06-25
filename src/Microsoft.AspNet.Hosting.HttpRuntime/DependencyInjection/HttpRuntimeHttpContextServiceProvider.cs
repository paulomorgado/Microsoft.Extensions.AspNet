using System;
using System.Web;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection.AspNet
{
    internal class HttpRuntimeHttpContextServiceProvider : IServiceProvider
    {
        private readonly HttpRuntimeRootServiceProvider root;
        private readonly ServiceProvider serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="HttpRuntimeServiceProvider"/>
        /// </summary>
        /// <param name="serviceProvider">The underlying <see cref="DependencyInjection.ServiceProvider"/>.</param>
        public HttpRuntimeHttpContextServiceProvider(HttpRuntimeRootServiceProvider root, ServiceProvider serviceProvider)
        {
            this.root = root;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType.-or- null if there is no service object of type serviceType.</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            return this.root.GetRootService(serviceType)
                ?? this.ResolveServiceProvider().GetService(serviceType)
                ?? this.root.GetOrAddRootService(serviceType);
        }

        /// <summary>
        /// Resolves the <see cref="IServiceProvider"/> for the current request.
        /// </summary>
        /// <returns>The resolved <see cref="IServiceProvider"/></returns>
        private IServiceProvider ResolveServiceProvider()
        {
            if (HttpContext.Current is null)
            {
                return this.serviceProvider;
            }

            if (HttpContext.Current.Items[HttpContextKeys.HttpContextServiceProviderScopeKey] is IServiceScope serviceScope)
            {
                return serviceScope.ServiceProvider;
            }

            serviceScope = this.CreateScope();

            HttpContext.Current.Items[HttpContextKeys.HttpContextServiceProviderScopeKey] = serviceScope;

            return serviceScope.ServiceProvider;
        }
    }
}
