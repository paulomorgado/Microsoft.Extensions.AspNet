using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi.Logging;
using Microsoft.Extensions.Logging;
using Debug = System.Diagnostics.Debug;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi.Logging
{
    /// <summary>
    /// Implementation of <see cref="ITraceWriter"/> that traces to <see cref="ILogger"/>
    /// </summary>
    internal sealed class LoggerTraceWritter : ITraceWriter
    {
        /// <summary>
        /// Duplicate of internal category name traced by WebApi for start/end of request.
        /// </summary>
        private const string SystemWebHttpRequestCategory = "System.Web.Http.Request";

        /// <summary>
        /// <see cref="TraceLevel"/> to <see cref="LogLevel"/> mapping.
        /// </summary>
        private static readonly LogLevel[] TraceLevelToLogLevel = new LogLevel[]
        {
            // TraceLevel.Off
            LogLevel.None,

            // TraceLevel.Debug
            LogLevel.Trace,

            // TraceLevel.Info
            LogLevel.Information,

            // TraceLevel.Warn
            LogLevel.Warning,

            // TraceLevel.Error
            LogLevel.Error,

            // TraceLevel.Fatal
            LogLevel.Critical
        };

        /// <summary>
        /// <see cref="TraceLevel"/> to <see cref="LogLevel"/> mapping.
        /// </summary>
        private static readonly EventId[] TraceKindToEventId = new EventId[]
        {
            // TraceKind.Trace
            new EventId((int)TraceKind.Trace, nameof(TraceKind.Trace)),

            // TraceKind.Begin
            new EventId((int)TraceKind.Begin, nameof(TraceKind.Begin)),

            // TraceKind.End
            new EventId((int)TraceKind.End, nameof(TraceKind.End)),
        };

        private static StringBuilder cachedStringBuilder;

        private readonly ConcurrentDictionary<string, ILogger> loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly Func<string, ILogger> loggerFactory;

        public LoggerTraceWritter(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory.CreateLogger;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {

            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (traceAction == null)
            {
                throw new ArgumentNullException(nameof(traceAction));
            }

            if (level < TraceLevel.Off || level > TraceLevel.Fatal)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "The TraceLevel property must be a value between TraceLevel.Off and TraceLevel.Fatal, inclusive.");
            }

            var logger = this.loggers.GetOrAdd(category, this.loggerFactory);

            LogLevel logLevel = TraceLevelToLogLevel[(int)level];

            if (!logger.IsEnabled(logLevel))
            {
                return;
            }

            var traceRecord = new TraceRecord(request, category, level);
            traceAction?.Invoke(traceRecord);

            var exception = traceRecord.Exception;

            var httpResponseException = ExtractHttpResponseException(exception);

            var loglevel = GetMappedLogLevel(httpResponseException, logLevel);

            // Level may have changed in Translate above
            if (!logger.IsEnabled(logLevel))
            {
                return;
            }

            var loggerRecord = Translate(traceRecord);

            TranslateHttpResponseException(loggerRecord, httpResponseException);

            Func<LoggerRecord, Exception, string> formatter;
            var isDebugLoggingEnabled = logger.IsEnabled(LogLevel.Debug);
            if (string.Equals(category, SystemWebHttpRequestCategory, StringComparison.Ordinal))
            {
                exception = null;
                formatter = (lr, e) => FormatSystemWebHttpRequestCategory(lr, e, isDebugLoggingEnabled);
            }
            else
            {
                if (loggerRecord.Kind == TraceKind.Begin && !isDebugLoggingEnabled)
                {
                    return;
                }

                formatter = (lr, e) => Format(lr, e, isDebugLoggingEnabled);

                if (!(httpResponseException is null))
                {
                    exception = null;
                }
            }

            logger.Log(
                logLevel,
                TraceKindToEventId[(int)loggerRecord.Kind],
                loggerRecord,
                exception,
                formatter);
        }

        private static LoggerRecord Translate(TraceRecord traceRecord)
        {
            var loggerRecord = new LoggerRecord
            {
                Kind = traceRecord.Kind == TraceKind.Begin || traceRecord.Kind == TraceKind.End ? traceRecord.Kind : TraceKind.Trace,
                Timestamp = traceRecord.Timestamp,
                Operator = traceRecord.Operator,
                Operation = traceRecord.Operation,
            };

            if (!(traceRecord.Request is null))
            {
                loggerRecord.Request = new RequestRecord
                {
                    Method = traceRecord.Request.Method.ToString(),
                    Uri = traceRecord.Request.RequestUri.ToString(),
                    Id = traceRecord.RequestId.ToString(),
                };
            }

            var status = (int)traceRecord.Status;
            if (status > 0)
            {
                loggerRecord.Response = new ResponseRecord
                {
                    Status = status,
                };
            }

            return loggerRecord;
        }

        /// <summary>
        /// Formats the given <see cref="LoggerRecord"/> into a string describing
        /// either the initial receipt of the incoming request or the final send
        /// of the response, depending on <see cref="TraceKind"/>.
        /// </summary>
        /// <param name="loggerRecord">The <see cref="LoggerRecord"/> from which to 
        /// produce the result.</param>
        /// <returns>A string containing comma-separated name-value pairs.</returns>
        /// <remarks>
        /// The first and last traces are injected by the tracing system itself.
        /// We use these to format unique strings identifying the incoming request
        /// and the outgoing response.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatSystemWebHttpRequestCategory(LoggerRecord loggerRecord, Exception exception, bool isDebugLoggingEnabled)
        {
            if (loggerRecord is null)
            {
                throw new ArgumentNullException(nameof(loggerRecord));
            }

            var messageBuilder = RentStringBuilder();

            if (isDebugLoggingEnabled)
            {
                messageBuilder
                    .Append('[')
                    .Append(FormatDateTime(loggerRecord.Timestamp))
                    .Append("] ")
                    .Append(loggerRecord.Kind == TraceKind.Begin ? "Request received" : "Sending response");
            }
            else
            {
                messageBuilder
                    .Append(loggerRecord.Kind == TraceKind.Begin ? "Request" : "Response");
            }

            FormatResponseStatus(loggerRecord.Response?.Status ?? 0, messageBuilder);

            if (!(loggerRecord.Request is null))
            {
                messageBuilder
                    .Append(", Method=")
                    .Append(loggerRecord.Request.Method);

                if (!string.IsNullOrEmpty(loggerRecord.Request.Uri))
                {
                    messageBuilder
                        .Append(", Url=")
                        .Append(loggerRecord.Request.Uri);
                }
            }

            var message = messageBuilder.ToString();

            ReturnStringBuilder(messageBuilder);

            return message;
        }

        private static string Format(LoggerRecord loggerRecord, Exception exception, bool isDebugLoggingEnabled)
        {
            if (loggerRecord is null)
            {
                throw new ArgumentNullException(nameof(loggerRecord));
            }

            var messageBuilder = RentStringBuilder();

            if (isDebugLoggingEnabled)
            {
                messageBuilder
                    .Append('[')
                    .Append(FormatDateTime(loggerRecord.Timestamp))
                    .Append("] Kind=")
                    .Append(loggerRecord.Kind.ToString());

                if (!(loggerRecord.Request is null))
                {
                    messageBuilder
                        .Append(", Id=")
                        .Append(loggerRecord.Request.Id.ToString());
                }
            }

            if (!(loggerRecord.Response?.HttpResponseError is null))
            {
                if (!string.IsNullOrEmpty(loggerRecord.Response.HttpResponseError.UserMessage))
                {
                    messageBuilder
                        .Append(", UserMessage='")
                        .Append(loggerRecord.Response.HttpResponseError.UserMessage)
                        .Append('\'');
                }

                if (!string.IsNullOrEmpty(loggerRecord.Response.HttpResponseError.MessageDetail))
                {
                    messageBuilder
                        .Append(", MessageDetail='")
                        .Append(loggerRecord.Response.HttpResponseError.MessageDetail)
                        .Append('\'');
                }

                if (!(loggerRecord.Response.HttpResponseError.HttpErrors is null))
                {
                    if (loggerRecord.Response.HttpResponseError.HttpErrors.Count == 0)
                    {
                        BuildHttpErrorMessage(messageBuilder, loggerRecord.Response.HttpResponseError.HttpErrors[0], string.Empty);
                    }
                    else
                    {
                        var i = -1;
                        foreach (var httpError in loggerRecord.Response.HttpResponseError.HttpErrors)
                        {
                            BuildHttpErrorMessage(messageBuilder, httpError, $"[{(++i).ToString(CultureInfo.InvariantCulture)}]");
                        }
                    }
                }

                if (!(loggerRecord.Response.HttpResponseError.ModelStateErrors is null)
                    && (loggerRecord.Response.HttpResponseError.ModelStateErrors.Count > 0))
                {
                    messageBuilder
                        .Append(", ModelStateError=[");

                    var i = -1;
                    foreach (var modelStateError in loggerRecord.Response.HttpResponseError.ModelStateErrors)
                    {
                        if (++i > 0)
                        {
                            messageBuilder
                                .Append(", ");
                        }

                        messageBuilder
                            .Append(modelStateError.Key);

                        if (!(modelStateError.Value is null) && (modelStateError.Value.Count > 0))
                        {
                            var j = -1;
                            foreach (var value in modelStateError.Value)
                            {
                                if (++j > 0)
                                {
                                    messageBuilder
                                        .Append(", ");
                                }

                                messageBuilder
                                    .Append(value);
                            }
                        }

                        messageBuilder
                            .Append(']');
                    }

                    messageBuilder
                        .Append(']');
                }
            }

            if (!string.IsNullOrEmpty(loggerRecord.Operator) && !string.IsNullOrEmpty(loggerRecord.Operation))
            {
                messageBuilder
                    .Append(", Operation=")
                    .Append(loggerRecord.Operator)
                    .Append('.')
                    .Append(loggerRecord.Operation);
            }

            FormatResponseStatus(loggerRecord.Response?.Status ?? 0, messageBuilder);

            var message = messageBuilder.ToString();

            ReturnStringBuilder(messageBuilder);

            return message;
        }

        private static void FormatResponseStatus(int status, StringBuilder messageBuilder)
        {
            if (status > 0)
            {
                messageBuilder
                    .Append(", Status=")
                    .Append(status)
                    .Append(" (")
                    .Append(((HttpStatusCode)status).ToString())
                    .Append(')');
            }
        }

        private static void BuildHttpErrorMessage(StringBuilder messageBuilder, HttpErrorRecord httpError, string index)
        {
            if (!string.IsNullOrEmpty(httpError.ExceptionType))
            {
                messageBuilder
                    .Append(", ExceptionType")
                    .Append(index)
                    .Append("='")
                    .Append(httpError.ExceptionType)
                    .Append('\'');
            }

            if (!string.IsNullOrEmpty(httpError.ExceptionMessage))
            {
                messageBuilder
                    .Append(", ExceptionMessage")
                    .Append(index)
                    .Append("='")
                    .Append(httpError.ExceptionMessage)
                    .Append('\'');
            }

            if (!string.IsNullOrEmpty(httpError.StackTrace))
            {
                messageBuilder
                    .Append(", StackTrace")
                    .Append(index)
                    .Append("='")
                    .Append(httpError.StackTrace)
                    .Append('\'');
            }
        }

        /// <summary>
        /// Examines the given <see cref="LoggerRecord"/> to determine whether it
        /// contains an <see cref="HttpResponseException"/> and if so, modifies
        /// the <see cref="LoggerRecord"/> to capture more detailed information.
        /// </summary>
        /// <param name="loggerRecord">The <see cref="LoggerRecord"/> to examine and modify.</param>
        private static void TranslateHttpResponseException(LoggerRecord loggerRecord, HttpResponseException httpResponseException)
        {
            if (httpResponseException == null)
            {
                return;
            }

            var response = httpResponseException.Response;
            Debug.Assert(!(response is null), $"'{response}' is null.");

            if (loggerRecord.Response is null)
            {
                loggerRecord.Response = new ResponseRecord();
            }

            // If the status has been set already, do not overwrite it,
            // otherwise propagate the status into the record.
            if (loggerRecord.Response.Status == 0)
            {
                loggerRecord.Response.Status = (int)response.StatusCode;
            }

            // HttpResponseExceptions often contain HttpError instances that carry
            // detailed information that may be filtered out by IncludeErrorDetailPolicy
            // before reaching the client. Capture it here for the trace.
            ObjectContent objectContent = response.Content as ObjectContent;
            if (objectContent == null)
            {
                return;
            }

            HttpError httpError = objectContent.Value as HttpError;
            if (httpError == null)
            {
                return;
            }

            loggerRecord.Response.HttpResponseError = new HttpResponseErrorRecord();

            if (httpError.TryGetValue(HttpErrorKeys.MessageKey, out var messageObject))
            {
                loggerRecord.Response.HttpResponseError.UserMessage = messageObject?.ToString();
            }

            if (httpError.TryGetValue(HttpErrorKeys.MessageDetailKey, out var messageDetailsObject))
            {
                loggerRecord.Response.HttpResponseError.MessageDetail = messageDetailsObject?.ToString();
            }

            // Extract the exception from this HttpError and then incrementally
            // walk down all inner exceptions.

            loggerRecord.Response.HttpResponseError.HttpErrors = new List<HttpErrorRecord>();

            while (httpError != null)
            {
                var httpErrorRecord = new HttpErrorRecord();

                if (httpError.TryGetValue(HttpErrorKeys.ExceptionTypeKey, out var exceptionTypeObject))
                {
                    httpErrorRecord.ExceptionType = exceptionTypeObject?.ToString();
                }

                if (httpError.TryGetValue(HttpErrorKeys.ExceptionMessageKey, out var exceptionMessageObject))
                {
                    httpErrorRecord.ExceptionMessage = exceptionMessageObject?.ToString();
                }

                if (httpError.TryGetValue(HttpErrorKeys.StackTraceKey, out var stackTraceObject))
                {
                    httpErrorRecord.StackTrace = stackTraceObject?.ToString();
                }

                loggerRecord.Response.HttpResponseError.HttpErrors.Add(httpErrorRecord);

                if (!httpError.TryGetValue(HttpErrorKeys.InnerExceptionKey, out var innerExceptionObject))
                {
                    break;
                }

                httpError = innerExceptionObject as HttpError;
            }

            // ModelState errors are handled with a nested HttpError
            if (httpError.TryGetValue(HttpErrorKeys.ModelStateKey, out var modelStateErrorObject))
            {
                HttpError modelStateError = modelStateErrorObject as HttpError;
                if (modelStateError != null)
                {
                    loggerRecord.Response.HttpResponseError.ModelStateErrors = new Dictionary<string, List<string>>();

                    foreach (var pair in modelStateError)
                    {
                        var errorList = pair.Value as IEnumerable<string>;

                        loggerRecord.Response.HttpResponseError.ModelStateErrors.Add(pair.Key, new List<string>(pair.Value as IEnumerable<string>));
                    }
                }
            }
        }

        private static HttpResponseException ExtractHttpResponseException(Exception exception)
        {
            switch (exception)
            {
                case null: return null;

                case HttpResponseException httpResponseException: return httpResponseException;

                case AggregateException aggregateException:

                    return aggregateException
                        .Flatten()
                        .InnerExceptions
                        .Select(ExtractHttpResponseException)
                        .Where(ex => ex != null && ex.Response != null)
                        .OrderByDescending(ex => ex.Response.StatusCode)
                        .FirstOrDefault();

                default: return ExtractHttpResponseException(exception.InnerException);
            }
        }

        /// <summary>
        /// Gets the <see cref="LogLevel"/> per the <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="httpResponseException">The exception for which the log level has to be found.</param>
        /// <param name="defaultLogLevel">The default log level.</param>
        /// <returns>The mapped log level if mapped; otherwise <paramref name="defaultLogLevel"/>.</returns>
        private static LogLevel? GetMappedLogLevel(HttpResponseException httpResponseException, LogLevel defaultLogLevel)
        {
            if (!(httpResponseException is null))
            {
                HttpResponseMessage response = httpResponseException.Response;
                Debug.Assert(!(response is null), $"'{response}' is null.");

                // Client level errors are downgraded to LogLevel.Warn
                if ((int)response.StatusCode < (int)HttpStatusCode.InternalServerError)
                {
                    return LogLevel.Warning;
                }

                // Non errors are downgraded to LogLevel.Info
                if ((int)response.StatusCode < (int)HttpStatusCode.BadRequest)
                {
                    return LogLevel.Information;
                }
            }

            return defaultLogLevel;
        }

        /// <summary>
        /// Formats a <see cref="DateTime"/> for the trace.
        /// </summary>
        /// <remarks>
        /// The default implementation uses the ISO 8601 convention
        /// for round-trippable dates so they can be parsed.
        /// </remarks>
        /// <param name="dateTime">The <see cref="DateTime"/></param>
        /// <returns>The <see cref="DateTime"/> formatted as a string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatDateTime(DateTime dateTime)
        {
            // The 'o' format is ISO 8601 for a round-trippable DateTime.
            // It is culture-invariant and can be parsed.
            return dateTime.ToString("o", CultureInfo.InvariantCulture);
        }

        private static StringBuilder RentStringBuilder()
        {
            return (Interlocked.Exchange(ref cachedStringBuilder, null) ?? new StringBuilder(8192)).Clear();
        }

        private static void ReturnStringBuilder(StringBuilder stringBuilder)
        {
            if (stringBuilder.Capacity <= 32768)
            {
                cachedStringBuilder = stringBuilder.Clear();
            }
        }
    }
}
