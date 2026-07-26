using Sarathi.API.Domain.Entities;

namespace Sarathi.API.Application.Interfaces;

public interface IAdminConfigurationRepository
{
    Task<AdminConfiguration?> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<AdminConfiguration> UpsertAsync(AdminConfiguration configuration, CancellationToken cancellationToken = default);
}
