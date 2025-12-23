using HnHMapperServer.Services.Interfaces;

namespace HnHMapperServer.Api.BackgroundServices;

/// <summary>
/// Background service that periodically flushes tenant activity timestamps to the database.
/// Runs every 2 minutes to persist cached activity data.
/// </summary>
public class TenantActivityFlushService : BackgroundService
{
    private readonly ITenantActivityService _activityService;
    private readonly ILogger<TenantActivityFlushService> _logger;
    private readonly TimeSpan _flushInterval = TimeSpan.FromMinutes(2);

    public TenantActivityFlushService(
        ITenantActivityService activityService,
        ILogger<TenantActivityFlushService> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Randomized startup delay to prevent all services starting simultaneously
        var startupDelay = TimeSpan.FromSeconds(Random.Shared.Next(10, 30));
        _logger.LogInformation("TenantActivityFlushService starting in {Delay:F1}s", startupDelay.TotalSeconds);
        await Task.Delay(startupDelay, stoppingToken);

        _logger.LogInformation("TenantActivityFlushService started. Flush interval: {Interval}", _flushInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_flushInterval, stoppingToken);

            try
            {
                await _activityService.FlushToDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing tenant activity to database");
            }
        }

        // Final flush on shutdown
        try
        {
            _logger.LogInformation("TenantActivityFlushService stopping - performing final flush");
            await _activityService.FlushToDatabaseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during final activity flush");
        }

        _logger.LogInformation("TenantActivityFlushService stopped");
    }
}
