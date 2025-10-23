using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyApp.WebAPI.HealthChecks
{
  /// <summary>
  /// Health check for monitoring system memory usage
  /// </summary>
  public class MemoryHealthCheck : IHealthCheck
  {
    private readonly ILogger<MemoryHealthCheck> _logger;
    private readonly long _maxMemoryBytes;

    public MemoryHealthCheck(
        ILogger<MemoryHealthCheck> logger,
        IConfiguration configuration)
    {
      _logger = logger;
      _maxMemoryBytes = configuration.GetValue<long>("HealthChecks:Memory:MaxMemoryMB", 512) * 1024 * 1024;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
      try
      {
        var currentMemory = GC.GetTotalMemory(false);
        var process = System.Diagnostics.Process.GetCurrentProcess();

        var healthData = new Dictionary<string, object>
                {
                    { "allocated_memory_mb", currentMemory / 1024 / 1024 },
                    { "working_set_mb", process.WorkingSet64 / 1024 / 1024 },
                    { "gc_gen0_collections", GC.CollectionCount(0) },
                    { "gc_gen1_collections", GC.CollectionCount(1) },
                    { "gc_gen2_collections", GC.CollectionCount(2) },
                    { "max_memory_mb", _maxMemoryBytes / 1024 / 1024 }
                };

        if (currentMemory > _maxMemoryBytes)
        {
          _logger.LogWarning(
              "High memory usage detected: {CurrentMemoryMB}MB (max: {MaxMemoryMB}MB)",
              currentMemory / 1024 / 1024, _maxMemoryBytes / 1024 / 1024);

          return Task.FromResult(HealthCheckResult.Degraded(
              $"High memory usage: {currentMemory / 1024 / 1024}MB",
              data: healthData));
        }

        var memoryPressure = currentMemory / (double)_maxMemoryBytes;
        if (memoryPressure > 0.8)
        {
          return Task.FromResult(HealthCheckResult.Degraded(
              $"Memory pressure at {memoryPressure:P0}",
              data: healthData));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Memory usage is normal", healthData));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Memory health check failed");
        return Task.FromResult(HealthCheckResult.Unhealthy("Memory health check failed", ex));
      }
    }
  }
}