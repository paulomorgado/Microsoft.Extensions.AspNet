using System.Net;
using System.Net.Http;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi.Logging
{
    public sealed class ResponseRecord
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> associated with the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public int Status { get; set; }

        public HttpResponseErrorRecord HttpResponseError { get; set; }
    }
}
