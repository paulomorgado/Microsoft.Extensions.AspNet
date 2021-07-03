using System;
using System.ComponentModel;
using System.Web.Http.Tracing;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi.Logging
{
    public sealed class LoggerRecord
    {
        /// <summary>
        /// Gets or sets the kind of trace.
        /// </summary>
        public TraceKind Kind { get; set; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> of this trace (via <see cref="DateTime.UtcNow"/>)
        /// </summary>
        public DateTime Timestamp { get; set; }

        public RequestRecord Request { get; set; }

        public ResponseRecord Response { get; set; }

        public string Operator { get; set; }

        public string Operation { get; set; }
    }
}
