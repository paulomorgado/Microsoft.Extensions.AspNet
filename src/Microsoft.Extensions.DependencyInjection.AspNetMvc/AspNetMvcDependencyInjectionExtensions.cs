using System;
using System.Web.Mvc;

namespace Microsoft.Extensions.DependencyInjection.AspNetMvc
{
    public static class AspNetMvcDependencyInjectionExtensions
    {
        public static IServiceProvider SetMvcDependencyResolver(this IServiceProvider serviceProvider)
        {
            DependencyResolver.SetResolver(new ServiceProviderMvcDependencyResolver(serviceProvider));

            return serviceProvider;
        }
    }
}
