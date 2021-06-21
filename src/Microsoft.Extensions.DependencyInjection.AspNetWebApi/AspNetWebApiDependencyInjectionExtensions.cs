using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Microsoft.Extensions.DependencyInjection.AspNetWebApi
{
    public static class AspNetWebApiDependencyInjectionExtensions
    {
        public static IServiceProvider SetWebApiDependencyResolver(this IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configure(config => config.DependencyResolver = new ServiceProviderWebApiDependencyResolver(serviceProvider));

            return serviceProvider;
        }
    }
}
