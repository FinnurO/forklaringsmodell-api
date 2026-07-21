using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class FaktumDto
{
    public Guid FaktumId { get; set; }
    public Guid SakId { get; set; }
    public Guid KildeId { get; set; }
    public FaktumType Type { get; set; }
    public StrukturType Struktur { get; set; }
    public string Verdi { get; set; } = string.Empty;
    public Guid? AvledetFraFaktumId { get; set; }
    public DateTimeOffset InnhentetTidspunkt { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
    public bool ErLaast { get; set; }
}

public class OpprettFaktumDto
{
    public Guid KildeId { get; set; }
    public FaktumType Type { get; set; } = FaktumType.Raatt;
    public StrukturType Struktur { get; set; }
    public string Verdi { get; set; } = string.Empty;
    public DateTimeOffset? InnhentetTidspunkt { get; set; }
    public List<Guid> RettskildeIder { get; set; } = new();
}

public class TransformerFaktumDto
{
    public string Verdi { get; set; } = string.Empty;
    public StrukturType Struktur { get; set; } = StrukturType.Strukturert;
    /// <summary>Kilde for det nye subsumerte faktumet. Hvis ikke satt, gjenbrukes kilden fra det opprinnelige faktumet.</summary>
    public Guid? KildeId { get; set; }
}
