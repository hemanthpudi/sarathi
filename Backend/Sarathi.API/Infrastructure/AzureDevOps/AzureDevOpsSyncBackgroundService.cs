using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Infrastructure.AzureDevOps;

public class AzureDevOpsSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AzureDevOpsSyncBackgroundService> _logger;

    public AzureDevOpsSyncBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<AzureDevOpsSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var runtimeService = scope.ServiceProvider.GetRequiredService<IAzureDevOpsSynchronizationRuntimeService>();
                await runtimeService.RunCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure DevOps background synchronization cycle failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}