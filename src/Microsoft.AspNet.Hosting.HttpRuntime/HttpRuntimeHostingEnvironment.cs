using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNet.Hosting
{
#pragma warning disable CS0618 // Type or member is obsolete
    internal class HttpRuntimeHostingEnvironment : IHttpRuntimeHostingEnvironment, Extensions.Hosting.IHostingEnvironment, IHttpRuntimeWebHostEnvironment
#pragma warning restore CS0618 // Type or member is obsolete
    {
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
