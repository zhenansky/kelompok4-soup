using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace MyApp.WebAPI.Controllers
{
  /// <summary>
  /// Controller for system diagnostics and monitoring
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class DiagnosticsController : ControllerBase
  {
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
      _logger = logger;
    }

    /// <summary>
    /// Get system information
    /// </summary>
    [HttpGet("system-info")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult<object> GetSystemInfo()
    {
      try
      {
        using var process = Process.GetCurrentProcess();

        var systemInfo = new
        {
          Application = new
          {
            Name = Assembly.GetEntryAssembly()?.GetName().Name,
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            StartTime = process.StartTime,
            Uptime = DateTime.Now - process.StartTime
          },
          System = new
          {
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            OSVersion = Environment.OSVersion.ToString(),
            RuntimeVersion = Environment.Version.ToString(),
            WorkingDirectory = Environment.CurrentDirectory
          },
          Memory = new
          {
            TotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
            WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
          },
          Process = new
          {
            Id = process.Id,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
          }
        };

        return Ok(systemInfo);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving system info");
        return StatusCode(500, new { error = "Error retrieving system info" });
      }
    }

    /// <summary>
    /// Force garbage collection (use with caution)
    /// </summary>
    [HttpPost("gc")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult ForceGarbageCollection()
    {
      try
      {
        _logger.LogWarning("Forced garbage collection requested by {User}", User?.Identity?.Name ?? "Anonymous");

        var memoryBefore = GC.GetTotalMemory(false);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryAfter = GC.GetTotalMemory(false);
        var memoryFreed = memoryBefore - memoryAfter;

        return Ok(new
        {
          message = "Garbage collection completed",
          memoryBeforeMB = memoryBefore / 1024 / 1024,
          memoryAfterMB = memoryAfter / 1024 / 1024,
          memoryFreedMB = memoryFreed / 1024 / 1024
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error forcing garbage collection");
        return StatusCode(500, new { error = "Error forcing garbage collection" });
      }
    }

    /// <summary>
    /// Test error handling (development only)
    /// </summary>
    [HttpGet("test-error")]
    [ProducesResponseType(typeof(object), 500)]
    public ActionResult TestError([FromQuery] string? type = "general")
    {
      _logger.LogInformation("Test error requested: {ErrorType}", type);

      Exception testException = type?.ToLower() switch
      {
        "database" => new InvalidOperationException("Test database connection error"),
        "memory" => new OutOfMemoryException("Test memory error"),
        "null" => new ArgumentNullException("testParameter", "Test null reference error"),
        "timeout" => new TimeoutException("Test timeout error"),
        _ => new ApplicationException("Test general error for monitoring")
      };

      throw testException;
    }

    /// <summary>
    /// Get current thread pool information
    /// </summary>
    [HttpGet("threadpool")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult GetThreadPoolInfo()
    {
      try
      {
        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
        ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);

        var threadPoolInfo = new
        {
          WorkerThreads = new
          {
            Available = workerThreads,
            Max = maxWorkerThreads,
            Min = minWorkerThreads,
            InUse = maxWorkerThreads - workerThreads
          },
          CompletionPortThreads = new
          {
            Available = completionPortThreads,
            Max = maxCompletionPortThreads,
            Min = minCompletionPortThreads,
            InUse = maxCompletionPortThreads - completionPortThreads
          },
          PendingWorkItemCount = ThreadPool.PendingWorkItemCount
        };

        return Ok(threadPoolInfo);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving thread pool info");
        return StatusCode(500, new { error = "Error retrieving thread pool info" });
      }
    }
  }
}