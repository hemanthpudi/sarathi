namespace Sarathi.API.Application.DTOs.Reports;

public class ReportSectionDto
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<ReportSectionSummaryDto> Summaries { get; set; } = new();

    public List<ReportSectionChartPointDto> ChartPoints { get; set; } = new();

    public List<string> Columns { get; set; } = new();

    public List<ReportTableRowDto> Rows { get; set; } = new();
}