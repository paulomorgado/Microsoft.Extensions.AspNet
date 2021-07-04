using System.Collections.Generic;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging
{
    public sealed class HttpResponseErrorRecord
    {
        public string UserMessage { get; set; }

        public string MessageDetail { get; set; }

        public List<HttpErrorRecord> HttpErrors { get; set; }

        public Dictionary<string, List<string>> ModelStateErrors { get; set; }
    }
}
