﻿using System;
using System.Web.Optimization;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    /// <summary>
    /// Contains extension methods to <see cref="IHttpRuntimeApplicationBuilder"/>.
    /// </summary>
    public static class BundlingApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds bundling to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IHttpRuntimeApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure the bundles.</param>
        /// <returns>The <see cref="IHttpRuntimeApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpRuntimeApplicationBuilder UseBundling(
            this IHttpRuntimeApplicationBuilder app,
            Action<BundleCollection> configureDelegate)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            configureDelegate(BundleTable.Bundles);

            return app;
        }
    }
}