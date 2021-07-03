using System;
using System.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Mvc
{
    /// <summary>
    /// Contains extension methods to <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class MvcApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMvc(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            DependencyResolver.SetResolver(new ServiceProviderMvcDependencyResolver(System.Web.HttpRuntime.WebObjectActivator));

            return app;
        }

        public static IApplicationBuilder UseMvc(this IApplicationBuilder app, Action<IMvcApplicationBuilder> configureDelegate)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMvc();

            var builder = new MvcApplicationBuilder(app.ApplicationServices);

            configureDelegate(builder);

            return app;
        }

        /// <summary>
        /// Adds all MVC Areas to the <see cref="IMvcApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="mvc">The <see cref="IMvcApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IMvcApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IMvcApplicationBuilder RegisterAllAreas(this IMvcApplicationBuilder mvc)
        {
            if (mvc == null)
            {
                throw new ArgumentNullException(nameof(mvc));
            }

            AreaRegistration.RegisterAllAreas();

            return mvc;
        }

        /// <summary>
        /// Adds all MVC Areas to the <see cref="IMvcApplicationBuilder"/> request execution pipeline by using user-defined information.
        /// </summary>
        /// <param name="mvc">The <see cref="IMvcApplicationBuilder"/>.</param>
        /// <param name="state">An object that contains user-defined information to pass to the area.</param>
        /// <returns>The <see cref="IMvcApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IMvcApplicationBuilder RegisterAllAreas(this IMvcApplicationBuilder mvc, object state)
        {
            if (mvc == null)
            {
                throw new ArgumentNullException(nameof(mvc));
            }

            AreaRegistration.RegisterAllAreas(state);

            return mvc;
        }

        /// <summary>
        /// Adds MVC global filters to the <see cref="IMvcApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="mvc">The <see cref="IMvcApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure the filters.</param>
        /// <returns>The <see cref="IMvcApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IMvcApplicationBuilder RegisterGlobalFilters(this IMvcApplicationBuilder mvc, Action<GlobalFilterCollection> configureDelegate)
        {
            if (mvc == null)
            {
                throw new ArgumentNullException(nameof(mvc));
            }

            configureDelegate(GlobalFilters.Filters);

            return mvc;
        }
    }
}
