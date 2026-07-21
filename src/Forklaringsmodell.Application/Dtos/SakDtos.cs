using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class SakDto
{
    public Guid SakId { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
    public DateTimeOffset Opprettet { get; set; }
    public DateTimeOffset SistEndret { get; set; }
}

public class OpprettSakDto
{
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; } = SakStatus.UnderBehandling;
}

public class OppdaterSakDto
{
    public string Tittel { get; set; } = string.Empty;
    public SakStatus Status { get; set; }
}
