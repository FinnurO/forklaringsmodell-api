using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class PartsmedvirkningDto
{
    public Guid MedvirkningId { get; set; }
    public Guid SakId { get; set; }
    public PartsmedvirkningType Type { get; set; }
    public DateTimeOffset Tidspunkt { get; set; }
    public string Innhold { get; set; } = string.Empty;
}

public class OpprettPartsmedvirkningDto
{
    public PartsmedvirkningType Type { get; set; }
    public DateTimeOffset? Tidspunkt { get; set; }
    public string Innhold { get; set; } = string.Empty;
}
