using Sarathi.API.Application.DTOs.Reports;

namespace Sarathi.API.Application.Interfaces;

public interface IReportsRepository
{
    Task<ReportsOverviewDto> GetOverviewAsync(Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default);

    Task<ExportReportResponseDto> ExportSectionAsync(string sectionKey, Guid? userId, string? role, int? projectId = null, CancellationToken cancellationToken = default);
}