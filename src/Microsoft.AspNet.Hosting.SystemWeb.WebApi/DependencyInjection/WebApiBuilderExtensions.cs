using System;
using System.Reflection;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for configuring Web API using an <see cref="IWebApiBuilder"/>.
    /// </summary>
    public static class WebApiBuilderExtensions
    {
        /// <summary>
        /// Adds an assembly reference.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="assembly">The <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IWebApiBuilder AddApplicationPart(this IWebApiBuilder builder, Assembly assembly)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return builder;
        }
    }
}
