using Serilog.Context;
using System.Diagnostics;

namespace MyApp.WebAPI.Middlewares
{
  /// <summary>
  /// Middleware for tracking correlation IDs across requests
  /// </summary>
  public class CorrelationIdMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string RequestIdHeaderName = "X-Request-ID";

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var correlationId = GetOrGenerateCorrelationId(context);
      var requestId = GenerateRequestId();

      // Set correlation ID in context
      context.TraceIdentifier = correlationId;

      // Add to response headers
      context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
      context.Response.Headers.TryAdd(RequestIdHeaderName, requestId);

      // Add to logging context
      using (LogContext.PushProperty("CorrelationId", correlationId))
      using (LogContext.PushProperty("RequestId", requestId))
      {
        // Add correlation info to Activity for distributed tracing
        var activity = Activity.Current;
        if (activity != null)
        {
          activity.SetTag("correlation.id", correlationId);
          activity.SetTag("request.id", requestId);
          activity.SetTag("http.method", context.Request.Method);
          activity.SetTag("http.path", context.Request.Path);
          activity.SetTag("user.id", context.User?.Identity?.Name ?? "anonymous");
        }

        _logger.LogDebug(
            "Request started - CorrelationId: {CorrelationId}, RequestId: {RequestId}",
            correlationId, requestId);

        try
        {
          await _next(context);
        }
        finally
        {
          _logger.LogDebug(
              "Request completed - CorrelationId: {CorrelationId}, RequestId: {RequestId}, StatusCode: {StatusCode}",
              correlationId, requestId, context.Response.StatusCode);
        }
      }
    }

    private string GetOrGenerateCorrelationId(HttpContext context)
    {
      // Try to get from request headers first
      if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
          !string.IsNullOrEmpty(correlationId))
      {
        return correlationId.ToString();
      }

      // Try alternative header names
      if (context.Request.Headers.TryGetValue("X-Trace-Id", out var traceId) &&
          !string.IsNullOrEmpty(traceId))
      {
        return traceId.ToString();
      }

      // Generate new correlation ID
      return GenerateCorrelationId();
    }

    private static string GenerateCorrelationId()
    {
      return Guid.NewGuid().ToString("N")[..16]; // 16-character hex string
    }

    private static string GenerateRequestId()
    {
      return Guid.NewGuid().ToString("N")[..8]; // 8-character hex string
    }
  }
}