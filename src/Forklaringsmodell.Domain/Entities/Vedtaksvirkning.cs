using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Én uavhengig, tidsbegrenset virkning av et Vedtak (tillatelse, plikt, økonomisk ytelse,
/// tilskudd, gebyr). Del av det frosne vedtaket og dermed append-only (regel 3.10).
/// </summary>
public class Vedtaksvirkning
{
    public Guid VirkningId { get; set; }
    public Guid VedtakId { get; set; }
    public Guid? VilkarId { get; set; } // valgfri kobling til katalogen, se Vilkar (regel 3.12)
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public Guid BeskrivelseTekstId { get; set; }
    public VarighetsType Varighet { get; set; }
    public DateTimeOffset? GyldigFra { get; set; }
    public DateTimeOffset? GyldigTil { get; set; } // skal være null når Varighet == Varig
    public decimal? Belop { get; set; }             // for OkonomiskYtelse/Tilskudd (til mottaker) eller Gebyr (fra mottaker)
    public Guid? LopendeVilkarTekstId { get; set; }
    public string? RapporteringsFrekvens { get; set; } // kun relevant når Type == Plikt
    public Guid? AvledetFraVirkningId { get; set; } // selvreferanse, kan peke på tvers av Vedtak/Sak (regel 3.13)

    public FlerspraakligTekst? BeskrivelseTekst { get; set; }
    public FlerspraakligTekst? LopendeVilkarTekst { get; set; }
    public Vedtak? Vedtak { get; set; }
    public Vilkar? Vilkar { get; set; }
    public Vedtaksvirkning? AvledetFraVirkning { get; set; }
    public ICollection<Vedtaksvirkning> AvledeteVirkninger { get; set; } = new List<Vedtaksvirkning>();
    public ICollection<VedtaksvirkningVurdering> VedtaksvirkningVurdering { get; set; } = new List<VedtaksvirkningVurdering>();
    public ICollection<VedtaksvirkningFaktum> VedtaksvirkningFaktum { get; set; } = new List<VedtaksvirkningFaktum>();
}
