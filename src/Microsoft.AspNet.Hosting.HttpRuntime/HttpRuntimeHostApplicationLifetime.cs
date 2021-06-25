using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Allows consumers to be notified of application lifetime events. This interface is not intended to be user-replaceable.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class HttpRuntimeHostApplicationLifetime : IHostApplicationLifetime, IApplicationLifetime
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <inheritdoc/>
        public CancellationToken ApplicationStarted { get; }

        /// <inheritdoc/>
        public CancellationToken ApplicationStopping { get; }

        /// <inheritdoc/>
        public CancellationToken ApplicationStopped { get; }

        /// <inheritdoc/>
        public void StopApplication()
        {
        }
    }
}
