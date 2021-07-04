using System;
using System.Reflection;
using Microsoft.AspNet.Hosting.SystemWeb.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for configuring MVC using an <see cref="IMvcBuilder"/>.
    /// </summary>
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Adds an assembly reference.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="assembly">The <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddApplicationPart(this IMvcBuilder builder, Assembly assembly)
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
