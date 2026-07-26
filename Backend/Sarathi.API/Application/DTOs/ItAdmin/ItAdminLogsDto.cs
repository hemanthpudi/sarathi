namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminLogsDto
{
    public int TotalLogs { get; set; }

    public int ErrorLogs { get; set; }

    public int WarningLogs { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ItAdminLogEntryDto> Items { get; set; } = new();
}