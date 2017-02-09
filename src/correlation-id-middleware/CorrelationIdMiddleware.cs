using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace CorrelationId
{
    public class CorrelationIdMiddleware
    {
        private CorrelationIdMiddlewareOptions _options;
        private RequestDelegate _next;

        private Microsoft.Extensions.Logging.ILogger _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger, CorrelationIdMiddlewareOptions options)
        {
            this._next = next;
            this._options = options;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string correlationId = context.TraceIdentifier;
            StringValues correlationHeader = new StringValues(correlationId);
            if (context.Request.Headers.TryGetValue(this._options.Header, out correlationHeader))
            {
                context.TraceIdentifier = correlationHeader.ToString();
            }
            await _next(context);
        }
    }

    public static class CorrelationIdMiddlewareExtension
    {
        public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder builder, CorrelationIdMiddlewareOptions options)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>(options);
        }
    }

    public class CorrelationIdMiddlewareOptions
    {
        public const string DefaultHeader = "X-Correlation-Id";
        public CorrelationIdMiddlewareOptions()
        {
            Header = DefaultHeader;
        }

        public string Header { get; set; }
    }
}