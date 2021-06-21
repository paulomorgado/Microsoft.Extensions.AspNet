using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Web;

namespace Microsoft.Extensions.DependencyInjection.AspNet
{
    /// <summary>
    /// Implements the object activator for <see cref="HttpApplication"/>s
    /// </summary>
    internal sealed class AspNetServiceProvider : IServiceProvider
    {
        private readonly AspNetRootServiceProvider root;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="AspNetServiceProvider"/>
        /// </summary>
        /// <param name="serviceProvider">The underlying <see cref="DependencyInjection.ServiceProvider"/>.</param>
        public AspNetServiceProvider(AspNetRootServiceProvider root, IServiceProvider serviceProvider)
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
                ?? this.serviceProvider.GetService(serviceType)
                ?? this.root.GetOrAddRootService(serviceType);
        }
    }
}
