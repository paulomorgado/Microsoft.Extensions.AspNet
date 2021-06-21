using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Extensions.DependencyInjection.AspNet
{
    public static class AspNetDependencyInjectionExtensions
    {
        public static IServiceProvider GetServiceProvider(this HttpContext httpContext)
            => (IServiceProvider)httpContext.Items[AspNetDependencyInjectionConfig.HttpContextServiceProviderKey];
    }
}
