namespace Sarathi.API.Application.DTOs.ItAdmin;

public class ItAdminSchedulingDto
{
    public int EnabledSchedules { get; set; }

    public int DisabledSchedules { get; set; }

    public int OverdueSchedules { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public List<ItAdminScheduleDto> Items { get; set; } = new();
}