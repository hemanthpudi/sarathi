namespace Sarathi.API.Application.Interfaces;

public interface IAzureDevOpsSynchronizationRuntimeService
{
    Task RunCycleAsync(CancellationToken cancellationToken = default);

    Task<bool> SynchronizeNowAsync(CancellationToken cancellationToken = default);
}