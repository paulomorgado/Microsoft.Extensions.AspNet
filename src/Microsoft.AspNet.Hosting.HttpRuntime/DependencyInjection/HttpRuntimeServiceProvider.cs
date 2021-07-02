using System;
using System.Web;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    /// <summary>
    /// Implements the object activator for <see cref="HttpApplication"/>s
    /// </summary>
    internal sealed class HttpRuntimeServiceProvider : IServiceProvider
    {
        private readonly HttpRuntimeRootServiceProvider root;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="HttpRuntimeServiceProvider"/>
        /// </summary>
        /// <param name="serviceProvider">The underlying <see cref="DependencyInjection.ServiceProvider"/>.</param>
        public HttpRuntimeServiceProvider(HttpRuntimeRootServiceProvider root, IServiceProvider serviceProvider)
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

            return root.GetRootService(serviceType)
                ?? serviceProvider.GetService(serviceType)
                ?? root.GetOrAddRootService(serviceType);
        }
    }
}
