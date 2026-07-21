using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Partsmedvirkning
{
    public Guid MedvirkningId { get; set; }
    public Guid SakId { get; set; }
    public PartsmedvirkningType Type { get; set; }
    public DateTimeOffset Tidspunkt { get; set; }
    public string Innhold { get; set; } = string.Empty;

    public bool ErLaast { get; set; }

    public Sak? Sak { get; set; }
}
