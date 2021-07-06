using System.Net;
using System.Net.Http;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging
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
