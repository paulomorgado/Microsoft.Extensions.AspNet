using System;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// A program abstraction.
    /// </summary>
    public interface IHttpRuntimeHost : IDisposable
    {
        /// <summary>
        /// The programs configured services.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Start the host on the specified <paramref name="httpApplication"/>.
        /// </summary>
        /// <param name="httpApplication">The <see cref="HttpApplication"/> instance to start the host on.</param>
        /// <returns>A <see cref="Task"/> that will be completed when the <see cref="IHttpRuntimeHost"/> starts.</returns>
        void Start(HttpApplication httpApplication);

        /// <summary>
        /// Stops the host.
        /// </summary>
        void Stop();
    }
}
