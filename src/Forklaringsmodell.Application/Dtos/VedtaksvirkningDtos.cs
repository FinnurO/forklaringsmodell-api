using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VedtaksvirkningDto
{
    public Guid VirkningId { get; set; }
    public Guid VedtakId { get; set; }
    public Guid? VilkarId { get; set; }
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string Beskrivelse { get; set; } = string.Empty;
    public VarighetsType Varighet { get; set; }
    public DateTimeOffset? GyldigFra { get; set; }
    public DateTimeOffset? GyldigTil { get; set; }
    public decimal? Belop { get; set; }
    public string? LopendeVilkar { get; set; }
    public string? RapporteringsFrekvens { get; set; }
    public Guid? AvledetFraVirkningId { get; set; }
    public List<Guid> VurderingIder { get; set; } = new();
    public List<Guid> FaktumIder { get; set; } = new();
}

/// <summary>Nøstet inne i OpprettVedtakDto.Virkninger — VedtakId settes serverside ved opprettelse.</summary>
public class OpprettVedtaksvirkningDto
{
    public Guid? VilkarId { get; set; }
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string Beskrivelse { get; set; } = string.Empty;
    public VarighetsType Varighet { get; set; }
    public DateTimeOffset? GyldigFra { get; set; }
    public DateTimeOffset? GyldigTil { get; set; }
    public decimal? Belop { get; set; }
    public string? LopendeVilkar { get; set; }
    public string? RapporteringsFrekvens { get; set; }
    public Guid? AvledetFraVirkningId { get; set; }
    public List<Guid> VurderingIder { get; set; } = new();
    public List<Guid> FaktumIder { get; set; } = new();
}
