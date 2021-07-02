using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
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

    public sealed class HttpErrorRecord
    {
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public sealed class RequestRecord
    {
        public string Uri { get; set; }
        public string Method { get; set; }
        public string Id { get; set; }
    }

    public sealed class ResponseRecord
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> associated with the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public int Status { get; set; }

        public HttpResponseErrorRecord HttpResponseError { get; set; }
    }

    public sealed class HttpResponseErrorRecord
    {
        public string UserMessage { get; set; }

        public string MessageDetail { get; set; }

        public List<HttpErrorRecord> HttpErrors { get; set; }

        public Dictionary<string, List<string>> ModelStateErrors { get; set; }
    }
}
