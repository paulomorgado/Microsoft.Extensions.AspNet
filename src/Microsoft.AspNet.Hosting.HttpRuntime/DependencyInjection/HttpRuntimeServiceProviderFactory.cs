using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    /// <summary>
    /// Default implementation of <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
    /// </summary>
    public class HttpRuntimeServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly ServiceProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRuntimeServiceProviderFactory"/> class
        /// with default options.
        /// </summary>
        public HttpRuntimeServiceProviderFactory() : this(new ServiceProviderOptions())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRuntimeServiceProviderFactory"/> class
        /// with the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options to use for this instance.</param>
        public HttpRuntimeServiceProviderFactory(ServiceProviderOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options;
        }

        /// <inheritdoc />
        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        /// <inheritdoc />
        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            var serviceProvider = containerBuilder.BuildServiceProvider();

            System.Web.HttpRuntime.WebObjectActivator = new WebObjectActivator(serviceProvider);

            return serviceProvider;
        }
    }
}
