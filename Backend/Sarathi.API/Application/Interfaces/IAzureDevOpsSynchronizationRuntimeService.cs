namespace Sarathi.API.Application.Interfaces;

public interface IAzureDevOpsSynchronizationRuntimeService
{
    Task RunCycleAsync(CancellationToken cancellationToken = default);
}