using Sarathi.API.Application.DTOs.Reports;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Application.Services;

public class ReportsService : IReportsService
{
    private readonly IReportsRepository _reportsRepository;

    public ReportsService(IReportsRepository reportsRepository)
    {
        _reportsRepository = reportsRepository;
    }

    public Task<ReportsOverviewDto> GetOverviewAsync(Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default)
    {
        return _reportsRepository.GetOverviewAsync(userId, role, projectId, cancellationToken);
    }

    public Task<ExportReportResponseDto> ExportSectionAsync(string sectionKey, Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSectionKey = string.IsNullOrWhiteSpace(sectionKey) ? "portfolio" : sectionKey.Trim().ToLowerInvariant();
        return _reportsRepository.ExportSectionAsync(normalizedSectionKey, userId, role, projectId, cancellationToken);
    }
}