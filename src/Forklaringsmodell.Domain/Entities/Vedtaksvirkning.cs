using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Én uavhengig, tidsbegrenset virkning av et Vedtak (tillatelse, plikt, økonomisk ytelse,
/// tilskudd). Del av det frosne vedtaket og dermed append-only (regel 3.10).
/// </summary>
public class Vedtaksvirkning
{
    public Guid VirkningId { get; set; }
    public Guid VedtakId { get; set; }
    public VirkningType Type { get; set; }
    public string Beskrivelse { get; set; } = string.Empty;
    public VarighetsType Varighet { get; set; }
    public DateTimeOffset? GyldigFra { get; set; }
    public DateTimeOffset? GyldigTil { get; set; } // skal være null når Varighet == Varig
    public decimal? Belop { get; set; }             // for OkonomiskYtelse/Tilskudd
    public string? LopendeVilkar { get; set; }
    public string? RapporteringsFrekvens { get; set; } // kun relevant når Type == Plikt

    public Vedtak? Vedtak { get; set; }
    public ICollection<VedtaksvirkningVurdering> VedtaksvirkningVurdering { get; set; } = new List<VedtaksvirkningVurdering>();
    public ICollection<VedtaksvirkningFaktum> VedtaksvirkningFaktum { get; set; } = new List<VedtaksvirkningFaktum>();
}
