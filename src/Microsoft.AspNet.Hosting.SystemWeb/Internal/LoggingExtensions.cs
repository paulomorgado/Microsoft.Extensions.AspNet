using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal static class LoggingExtensions
    {
        private static readonly Func<ILogger, Guid, IDisposable> beginApplicationInstanceScopeAction = LoggerMessage.DefineScope<Guid>("Application {ApplicationId}");
        private static readonly Func<ILogger, Guid, IDisposable> beginRequestScopeAction = LoggerMessage.DefineScope<Guid>("Request {RequestId}");
        private static readonly Action<ILogger, Guid, string, Uri, Exception> beginRequestMessageAction = LoggerMessage.Define<Guid, string, Uri>(LogLevel.Information, new EventId(1, "RequestStarting"), "Request starting ({RequestId}) {HttpMethod} {Uri}");
        private static readonly Action<ILogger, Guid, int, Exception> endRequestMessageAction = LoggerMessage.Define<Guid, int>(LogLevel.Information, new EventId(2, "RequestFinished"), "Request finished ({RequestId}) {StatusCode}");

        public static IDisposable BeginApplicationInstanceScope(this ILogger logger, Guid id) => beginApplicationInstanceScopeAction(logger, id);

        public static IDisposable BeginRequestLoggerScope(this ILogger logger, Guid id) => beginRequestScopeAction(logger, id);

        public static void LogBeginRequest(this ILogger logger, Guid id, Uri uri, string httpMethod) => beginRequestMessageAction(logger, id, httpMethod, uri, null);

        public static void LogEndRequest(this ILogger logger, Guid id, int status) => endRequestMessageAction(logger, id, status, null);
    }
}
