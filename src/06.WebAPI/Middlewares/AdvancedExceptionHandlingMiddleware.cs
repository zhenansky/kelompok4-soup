using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace MyApp.WebAPI.Middlewares
{
  /// <summary>
  /// Advanced exception handling middleware with detailed error tracking
  /// </summary>
  public class AdvancedExceptionHandlingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<AdvancedExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private static readonly ActivitySource ActivitySource = new("MyApp.ExceptionHandling");

    public AdvancedExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<AdvancedExceptionHandlingMiddleware> logger,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
      _next = next;
      _logger = logger;
      _environment = environment;
      _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      using var activity = ActivitySource.StartActivity("ExceptionHandling");

      try
      {
        await _next(context);
      }
      catch (Exception ex)
      {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        await HandleExceptionAsync(context, ex, activity);
      }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, Activity? activity)
    {
      var errorId = Guid.NewGuid().ToString();

      // Create comprehensive error context
      var errorContext = new ErrorContext
      {
        ErrorId = errorId,
        Timestamp = DateTime.UtcNow,
        RequestPath = context.Request.Path.Value ?? "",
        RequestMethod = context.Request.Method,
        QueryString = context.Request.QueryString.Value ?? "",
        UserAgent = context.Request.Headers["User-Agent"].ToString(),
        IpAddress = GetClientIpAddress(context),
        UserId = GetUserId(context),
        Username = GetUsername(context),
        TraceId = context.TraceIdentifier,
        ActivityId = activity?.Id,
        Exception = exception,
        RequestHeaders = GetSafeHeaders(context.Request.Headers),
        StackTrace = exception.StackTrace,
        InnerExceptions = GetInnerExceptions(exception)
      };

      // Enhanced logging with structured data
      using var scope = _logger.BeginScope(new Dictionary<string, object>
      {
        ["ErrorId"] = errorId,
        ["TraceId"] = context.TraceIdentifier,
        ["UserId"] = errorContext.UserId ?? "anonymous",
        ["RequestPath"] = errorContext.RequestPath,
        ["ExceptionType"] = exception.GetType().Name
      });

      // Log different levels based on exception type
      var logLevel = GetLogLevel(exception);
      _logger.Log(logLevel, exception,
          "Unhandled exception occurred. ErrorId: {ErrorId}, Path: {RequestPath}, User: {UserId}, Type: {ExceptionType}",
          errorId, errorContext.RequestPath, errorContext.UserId, exception.GetType().Name);

      // Add activity tags for distributed tracing
      activity?.SetTag("error.id", errorId);
      activity?.SetTag("error.type", exception.GetType().Name);
      activity?.SetTag("error.message", exception.Message);
      activity?.SetTag("http.request.path", errorContext.RequestPath);
      activity?.SetTag("user.id", errorContext.UserId);

      // Determine response based on exception type and environment
      var (statusCode, response) = CreateErrorResponse(exception, errorContext);

      // Set response
      context.Response.StatusCode = (int)statusCode;
      context.Response.ContentType = "application/json";

      // Add error tracking headers (safe for production)
      context.Response.Headers["X-Error-Id"] = errorId;
      context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;

      if (_environment.IsDevelopment())
      {
        context.Response.Headers["X-Error-Type"] = exception.GetType().Name;
      }

      var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = _environment.IsDevelopment()
      });

      await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);

      // Save detailed error information for debugging (development only)
      if (_environment.IsDevelopment())
      {
        await SaveDetailedErrorInfoAsync(errorContext);
      }
    }

    private (HttpStatusCode statusCode, object response) CreateErrorResponse(Exception exception, ErrorContext context)
    {
      return exception switch
      {
        ArgumentNullException or ArgumentException => (
            HttpStatusCode.BadRequest,
            CreateErrorResponseObject("INVALID_REQUEST", "The request contains invalid parameters.", context.ErrorId,
                _environment.IsDevelopment() ? exception.Message : null)
        ),
        UnauthorizedAccessException => (
            HttpStatusCode.Unauthorized,
            CreateErrorResponseObject("UNAUTHORIZED", "Access denied.", context.ErrorId)
        ),
        KeyNotFoundException => (
            HttpStatusCode.NotFound,
            CreateErrorResponseObject("NOT_FOUND", "The requested resource was not found.", context.ErrorId)
        ),
        TimeoutException => (
            HttpStatusCode.RequestTimeout,
            CreateErrorResponseObject("TIMEOUT", "The request timed out.", context.ErrorId)
        ),
        InvalidOperationException => (
            HttpStatusCode.Conflict,
            CreateErrorResponseObject("INVALID_OPERATION", "The requested operation is not valid in the current state.", context.ErrorId,
                _environment.IsDevelopment() ? exception.Message : null)
        ),
        _ => (
            HttpStatusCode.InternalServerError,
            CreateErrorResponseObject("INTERNAL_ERROR", "An internal server error occurred.", context.ErrorId,
                _environment.IsDevelopment() ? exception.Message : null)
        )
      };
    }

    private object CreateErrorResponseObject(string code, string message, string errorId, string? details = null)
    {
      var response = new
      {
        Error = new
        {
          Code = code,
          Message = message,
          ErrorId = errorId,
          Timestamp = DateTime.UtcNow
        }
      };

      if (_environment.IsDevelopment() && !string.IsNullOrEmpty(details))
      {
        return new
        {
          response.Error.Code,
          response.Error.Message,
          response.Error.ErrorId,
          response.Error.Timestamp,
          Details = details,
          Environment = "Development"
        };
      }

      return response;
    }

    private LogLevel GetLogLevel(Exception exception)
    {
      return exception switch
      {
        ArgumentException or ArgumentNullException => LogLevel.Warning,
        UnauthorizedAccessException => LogLevel.Warning,
        KeyNotFoundException => LogLevel.Information,
        TimeoutException => LogLevel.Warning,
        _ => LogLevel.Error
      };
    }

    private string? GetClientIpAddress(HttpContext context)
    {
      var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
      if (string.IsNullOrEmpty(ipAddress))
      {
        ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
      }
      if (string.IsNullOrEmpty(ipAddress))
      {
        ipAddress = context.Connection.RemoteIpAddress?.ToString();
      }
      return ipAddress;
    }

    private string? GetUserId(HttpContext context)
    {
      return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetUsername(HttpContext context)
    {
      return context.User?.FindFirst(ClaimTypes.Name)?.Value ??
             context.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
      var safeHeaders = new Dictionary<string, string>();
      var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key", "x-auth-token" };

      foreach (var header in headers)
      {
        var key = header.Key.ToLower();
        if (sensitiveHeaders.Contains(key))
        {
          safeHeaders[header.Key] = "***REDACTED***";
        }
        else
        {
          safeHeaders[header.Key] = header.Value.ToString();
        }
      }

      return safeHeaders;
    }

    private List<ExceptionInfo> GetInnerExceptions(Exception exception)
    {
      var innerExceptions = new List<ExceptionInfo>();
      var current = exception.InnerException;
      var depth = 0;

      while (current != null && depth < 10) // Prevent infinite loops
      {
        innerExceptions.Add(new ExceptionInfo
        {
          Type = current.GetType().Name,
          Message = current.Message,
          StackTrace = _environment.IsDevelopment() ? current.StackTrace : null,
          Depth = depth
        });

        current = current.InnerException;
        depth++;
      }

      return innerExceptions;
    }

    private async Task SaveDetailedErrorInfoAsync(ErrorContext errorContext)
    {
      try
      {
        var errorDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "errors");
        Directory.CreateDirectory(errorDirectory);

        var fileName = $"error_{errorContext.ErrorId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(errorDirectory, fileName);

        var detailedInfo = new
        {
          errorContext.ErrorId,
          errorContext.Timestamp,
          errorContext.RequestPath,
          errorContext.RequestMethod,
          errorContext.QueryString,
          errorContext.UserId,
          errorContext.Username,
          errorContext.IpAddress,
          errorContext.UserAgent,
          errorContext.TraceId,
          errorContext.ActivityId,
          Exception = new
          {
            Type = errorContext.Exception.GetType().FullName,
            Message = errorContext.Exception.Message,
            StackTrace = errorContext.Exception.StackTrace,
            Data = errorContext.Exception.Data,
            HResult = errorContext.Exception.HResult,
            Source = errorContext.Exception.Source,
            TargetSite = errorContext.Exception.TargetSite?.ToString()
          },
          errorContext.InnerExceptions,
          errorContext.RequestHeaders,
          Environment = new
          {
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            WorkingSet = Environment.WorkingSet,
            GcMemory = GC.GetTotalMemory(false)
          }
        };

        var json = JsonSerializer.Serialize(detailedInfo, new JsonSerializerOptions
        {
          WriteIndented = true,
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(filePath, json);

        _logger.LogDebug("Detailed error information saved to: {FilePath}", filePath);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to save detailed error information for ErrorId: {ErrorId}", errorContext.ErrorId);
      }
    }
  }

  // Supporting Models
  public class ErrorContext
  {
    public string ErrorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public Exception Exception { get; set; } = null!;
    public Dictionary<string, string> RequestHeaders { get; set; } = new();
    public string? StackTrace { get; set; }
    public List<ExceptionInfo> InnerExceptions { get; set; } = new();
  }

  public class ExceptionInfo
  {
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int Depth { get; set; }
  }
}