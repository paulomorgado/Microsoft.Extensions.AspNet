﻿namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging
{
    public sealed class HttpErrorRecord
    {
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
    }
}
