using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class HostingEnvironment : 
        AspNetCore.Hosting.IHostingEnvironment, 
#pragma warning disable CS0618 // Type or member is obsolete
        Extensions.Hosting.IHostingEnvironment, 
#pragma warning restore CS0618 // Type or member is obsolete
        IWebHostEnvironment
    {
#if DEBUG
        public HostingEnvironment()
        {
        }
#endif

        /// <inheritdoc/>
        public string EnvironmentName { get; set; } = Extensions.Hosting.Environments.Production;

        /// <inheritdoc/>
        public string ApplicationName { get; set; } = default;

        /// <inheritdoc/>
        public string WebRootPath { get; set; } = default;

        /// <inheritdoc/>
        public IFileProvider WebRootFileProvider { get; set; } = default;

        /// <inheritdoc/>
        public string ContentRootPath { get; set; } = default;

        /// <inheritdoc/>
        public IFileProvider ContentRootFileProvider { get; set; } = default;
    }
}
