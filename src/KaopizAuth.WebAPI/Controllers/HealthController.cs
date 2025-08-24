using KaopizAuth.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.WebAPI.Controllers;

/// <summary>
/// Health check controller for monitoring application health
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Basic health status</returns>
    [HttpGet]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "KaopizAuth API",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }

    /// <summary>
    /// Readiness probe for Kubernetes/container orchestration
    /// </summary>
    /// <returns>Readiness status</returns>
    [HttpGet("ready")]
    public async Task<ActionResult<object>> GetReadiness()
    {
        try
        {
            // Check if the application is ready to receive traffic
            var dbConnected = await CheckDatabaseConnection();

            if (!dbConnected)
            {
                return ServiceUnavailable(new
                {
                    Status = "NotReady",
                    Reason = "Database connection failed",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Status = "Ready",
                Database = "Connected",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return ServiceUnavailable(new
            {
                Status = "NotReady",
                Reason = "Internal error",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Liveness probe for Kubernetes/container orchestration
    /// </summary>
    /// <returns>Liveness status</returns>
    [HttpGet("live")]
    public ActionResult<object> GetLiveness()
    {
        // Simple liveness check - if this endpoint responds, the application is alive
        return Ok(new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            ProcessId = Environment.ProcessId,
            MachineName = Environment.MachineName
        });
    }

    /// <summary>
    /// Database health check endpoint
    /// </summary>
    /// <returns>Database connection status</returns>
    [HttpGet("database")]
    public async Task<ActionResult<object>> GetDatabaseHealth()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var canConnect = await CheckDatabaseConnection();
            stopwatch.Stop();

            if (canConnect)
            {
                return Ok(new
                {
                    Status = "Healthy",
                    Database = "Connected",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return ServiceUnavailable(new
                {
                    Status = "Unhealthy",
                    Database = "Disconnected",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return ServiceUnavailable(new
            {
                Status = "Unhealthy",
                Database = "Error",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Comprehensive health check with all dependencies
    /// </summary>
    /// <returns>Complete health status</returns>
    [HttpGet("detailed")]
    public async Task<ActionResult<object>> GetDetailedHealth()
    {
        var healthChecks = new Dictionary<string, object>();
        var overallStatus = "Healthy";

        // Database check
        try
        {
            var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var dbConnected = await CheckDatabaseConnection();
            dbStopwatch.Stop();

            healthChecks["Database"] = new
            {
                Status = dbConnected ? "Healthy" : "Unhealthy",
                ResponseTimeMs = dbStopwatch.ElapsedMilliseconds
            };

            if (!dbConnected) overallStatus = "Degraded";
        }
        catch (Exception ex)
        {
            healthChecks["Database"] = new
            {
                Status = "Unhealthy",
                Error = ex.Message
            };
            overallStatus = "Unhealthy";
        }

        // Memory check
        var totalMemory = GC.GetTotalMemory(false);
        healthChecks["Memory"] = new
        {
            Status = totalMemory < 500_000_000 ? "Healthy" : "Warning", // 500MB threshold
            TotalMemoryBytes = totalMemory,
            TotalMemoryMB = totalMemory / (1024 * 1024)
        };

        // Disk space check (logs directory)
        try
        {
            var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
            if (Directory.Exists(logsPath))
            {
                var drive = new DriveInfo(Path.GetPathRoot(logsPath) ?? "/");
                var freeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);

                healthChecks["DiskSpace"] = new
                {
                    Status = freeSpaceGB > 1 ? "Healthy" : "Warning", // 1GB threshold
                    AvailableFreeSpaceGB = freeSpaceGB,
                    TotalSizeGB = drive.TotalSize / (1024 * 1024 * 1024)
                };
            }
        }
        catch (Exception ex)
        {
            healthChecks["DiskSpace"] = new
            {
                Status = "Warning",
                Error = ex.Message
            };
        }

        return overallStatus == "Unhealthy"
            ? ServiceUnavailable(new
            {
                Status = overallStatus,
                Checks = healthChecks,
                Timestamp = DateTime.UtcNow
            })
            : Ok(new
            {
                Status = overallStatus,
                Checks = healthChecks,
                Timestamp = DateTime.UtcNow
            });
    }

    private async Task<bool> CheckDatabaseConnection()
    {
        try
        {
            // Try to execute a simple query to check database connectivity
            var userCount = await _context.Users.CountAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private ActionResult<object> ServiceUnavailable(object response)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}