using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal static class HostingEnvironmentExtensions
    {
        internal static void Initialize(this IWebHostEnvironment hostingEnvironment, string contentRootPath, WebHostOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(contentRootPath))
            {
                throw new ArgumentException("A valid non-empty content root must be provided.", nameof(contentRootPath));
            }
            if (!Directory.Exists(contentRootPath))
            {
                throw new ArgumentException($"The content root '{contentRootPath}' does not exist.", nameof(contentRootPath));
            }

            hostingEnvironment.ApplicationName = options.ApplicationName;
            hostingEnvironment.ContentRootPath = contentRootPath;
            hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);

            var webRoot = options.WebRoot;
            if (webRoot == null)
            {
                hostingEnvironment.WebRootPath = hostingEnvironment.ContentRootPath;
            }
            else
            {
                hostingEnvironment.WebRootPath = Path.Combine(hostingEnvironment.ContentRootPath, webRoot);
            }

            if (!string.IsNullOrEmpty(hostingEnvironment.WebRootPath))
            {
                hostingEnvironment.WebRootPath = Path.GetFullPath(hostingEnvironment.WebRootPath);
                if (!Directory.Exists(hostingEnvironment.WebRootPath))
                {
                    Directory.CreateDirectory(hostingEnvironment.WebRootPath);
                }
                hostingEnvironment.WebRootFileProvider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
            }
            else
            {
                hostingEnvironment.WebRootFileProvider = new NullFileProvider();
            }

            hostingEnvironment.EnvironmentName =
                options.Environment ??
                hostingEnvironment.EnvironmentName;
        }
    }
}
