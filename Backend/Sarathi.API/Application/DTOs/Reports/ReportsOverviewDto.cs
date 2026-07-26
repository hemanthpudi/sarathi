namespace Sarathi.API.Application.DTOs.Reports;

public class ReportsOverviewDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public List<ReportSectionDto> Sections { get; set; } = new();
}