namespace Sarathi.API.Application.DTOs.ItAdmin;

public class TriggerItAdminSyncRequestDto
{
    public string SyncType { get; set; } = "Full";

    public string ScopeName { get; set; } = "All Projects";

    public string Source { get; set; } = "Manual";
}