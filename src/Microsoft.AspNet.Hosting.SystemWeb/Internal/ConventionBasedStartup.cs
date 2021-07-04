using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNet.Hosting.SystemWeb.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class ConventionBasedStartup : IStartup
    {
        private readonly StartupMethods methods;

        public ConventionBasedStartup(StartupMethods methods)
        {
            this.methods = methods;
        }

        public void Configure(IApplicationBuilder app)
        {
            try
            {
                methods.ConfigureDelegate(app);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                return methods.ConfigureServicesDelegate(services);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }
    }
}
