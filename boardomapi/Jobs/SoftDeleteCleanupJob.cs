using boardomapi.Database;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Jobs;

public class SoftDeleteCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SoftDeleteCleanupJob> _logger;

    public SoftDeleteCleanupJob(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SoftDeleteCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _configuration.GetValue<int?>("CleanupJob:IntervalMinutes");
        var intervalDays = _configuration.GetValue<int>("CleanupJob:IntervalDays");
        var interval = intervalMinutes.HasValue
            ? TimeSpan.FromMinutes(intervalMinutes.Value)
            : TimeSpan.FromDays(intervalDays);

        if (interval < MinInterval)
        {
            _logger.LogWarning(
                "SoftDeleteCleanupJob: Configured interval {Interval} is too small, defaulting to {MinInterval}",
                interval, MinInterval);
            interval = MinInterval;
        }

        _logger.LogInformation("SoftDeleteCleanupJob started. Running every {Interval}", interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);

            try
            {
                await PurgeSoftDeletedEntitiesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while purging soft-deleted entities");
            }
        }
    }

    private async Task PurgeSoftDeletedEntitiesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deletedDevices = await db.Devices
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted)
            .ToListAsync(stoppingToken);

        var deletedGroups = await db.Groups
            .IgnoreQueryFilters()
            .Where(g => g.IsDeleted)
            .ToListAsync(stoppingToken);

        if (deletedDevices.Count == 0 && deletedGroups.Count == 0)
        {
            _logger.LogInformation("SoftDeleteCleanupJob: No soft-deleted entities found");
            return;
        }

        db.Devices.RemoveRange(deletedDevices);
        db.Groups.RemoveRange(deletedGroups);

        await db.SaveChangesAsync(stoppingToken);

        _logger.LogInformation(
            "SoftDeleteCleanupJob: Purged {DeviceCount} device(s) and {GroupCount} group(s)",
            deletedDevices.Count,
            deletedGroups.Count);
    }
}
