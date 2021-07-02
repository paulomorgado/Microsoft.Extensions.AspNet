using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal static class WebHostUtilities
    {
        public static bool ParseBool(this IConfiguration configuration, string key)
        {
            return string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase)
                || string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
        }
    }
}
