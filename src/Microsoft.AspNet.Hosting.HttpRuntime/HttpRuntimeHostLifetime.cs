using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting
{
    public class HttpRuntimeHostLifetime : IHostLifetime
    {
        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
