using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MyApp.WebAPI.Data;

namespace MyApp.WebAPI.HealthChecks
{
  /// <summary>
  /// Custom health check for database connectivity and performance
  /// </summary>
  public class DatabaseHealthCheck : IHealthCheck
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        ApplicationDbContext context,
        ILogger<DatabaseHealthCheck> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
      try
      {
        _logger.LogDebug("Starting database health check");

        // Test basic connectivity
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
          return HealthCheckResult.Unhealthy("Cannot connect to database");
        }

        // Check database performance
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var count = await _context.Users.CountAsync(cancellationToken);
        stopwatch.Stop();

        var healthData = new Dictionary<string, object>
                {
                    { "user_count", count },
                    { "response_time_ms", stopwatch.ElapsedMilliseconds },
                    { "connection_state", "connected" }
                };

        if (stopwatch.ElapsedMilliseconds > 1000)
        {
          return HealthCheckResult.Degraded(
              $"Slow database response: {stopwatch.ElapsedMilliseconds}ms",
              data: healthData);
        }

        _logger.LogDebug("Database health check completed successfully");
        return HealthCheckResult.Healthy("Database is healthy", healthData);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Database health check failed");
        return HealthCheckResult.Unhealthy("Database health check failed", ex);
      }
    }
  }
}