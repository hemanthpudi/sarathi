namespace Sarathi.API.Application.DTOs.Reports;

public class ExportReportResponseDto
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "text/csv";

    public string ContentBase64 { get; set; } = string.Empty;
}