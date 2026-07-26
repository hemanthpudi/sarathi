using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Persistence;

namespace Sarathi.API.Infrastructure.Repositories;

public class AdminConfigurationRepository : IAdminConfigurationRepository
{
    private readonly AppDbContext _dbContext;

    public AdminConfigurationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminConfiguration?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.AdminConfigurations
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminConfiguration> UpsertAsync(AdminConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.AdminConfigurations
            .FirstOrDefaultAsync(item => item.IsActive, cancellationToken);

        if (existing is null)
        {
            _dbContext.AdminConfigurations.Add(configuration);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return configuration;
        }

        existing.KpiRefreshIntervalMinutes = configuration.KpiRefreshIntervalMinutes;
        existing.SprintVelocityTarget = configuration.SprintVelocityTarget;
        existing.CompletionRateTarget = configuration.CompletionRateTarget;
        existing.DefectDensityThreshold = configuration.DefectDensityThreshold;
        existing.BacklogHealthThreshold = configuration.BacklogHealthThreshold;
        existing.ReleaseSuccessRateTarget = configuration.ReleaseSuccessRateTarget;
        existing.AzureDevOpsOrganizationUrl = configuration.AzureDevOpsOrganizationUrl;
        existing.AzureDevOpsProjectFilter = configuration.AzureDevOpsProjectFilter;
        existing.AzureDevOpsSyncEnabled = configuration.AzureDevOpsSyncEnabled;
        existing.AzureDevOpsPatCipherText = configuration.AzureDevOpsPatCipherText;
        existing.AzureDevOpsPatUpdatedAtUtc = configuration.AzureDevOpsPatUpdatedAtUtc;
        existing.UpdatedAtUtc = configuration.UpdatedAtUtc;
        existing.UpdatedByUserId = configuration.UpdatedByUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
