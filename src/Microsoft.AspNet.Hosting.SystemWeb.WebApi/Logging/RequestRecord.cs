﻿namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging
{
    public sealed class RequestRecord
    {
        public string Uri { get; set; }
        public string Method { get; set; }
        public string Id { get; set; }
    }
}
