using System;
using System.Web;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Represents a configured web host.
    /// </summary>
    public interface IHttpRuntimeWebHost
    {
        /// <summary>
        /// The <see cref="IServiceProvider"/> for the host.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Starts listening on <see cref="HttpApplication"/> events.
        /// </summary>
        /// <param name="httpApplication">The HTTP application.</param>
        void Start(HttpApplication httpApplication);
    }
}
