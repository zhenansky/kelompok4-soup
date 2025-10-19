using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MyApp.WebAPI.HealthChecks
{
  /// <summary>
  /// Custom response writer for health check results
  /// </summary>
  public static class HealthCheckResponseWriter
  {
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true
    };

    public static async Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
      context.Response.ContentType = "application/json; charset=utf-8";

      var response = new HealthCheckResponse
      {
        Status = healthReport.Status.ToString(),
        TotalDuration = healthReport.TotalDuration,
        Timestamp = DateTime.UtcNow,
        Checks = healthReport.Entries.Select(entry => new HealthCheckInfo
        {
          Name = entry.Key,
          Status = entry.Value.Status.ToString(),
          Description = entry.Value.Description,
          Duration = entry.Value.Duration,
          Exception = entry.Value.Exception?.Message,
          Data = entry.Value.Data
        }).ToList()
      };

      var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
      await context.Response.WriteAsync(jsonResponse);
    }
  }

  public class HealthCheckResponse
  {
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public DateTime Timestamp { get; set; }
    public List<HealthCheckInfo> Checks { get; set; } = new();
  }

  public class HealthCheckInfo
  {
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Exception { get; set; }
    public IReadOnlyDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
  }
}