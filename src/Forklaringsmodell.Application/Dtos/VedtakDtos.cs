using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Dtos;

public class VedtakDto
{
    public Guid VedtakId { get; set; }
    public Guid SakId { get; set; }
    public DateTimeOffset Tidspunkt { get; set; }
    public string Utfall { get; set; } = string.Empty;
    public AutomatiseringsGrad AutomatiseringsGrad { get; set; }
}

public class OpprettVedtakDto
{
    public string Utfall { get; set; } = string.Empty;
    public List<Guid> FaktumIder { get; set; } = new();
    public List<Guid> VurderingIder { get; set; } = new();
    public List<Guid> PartsmedvirkningIder { get; set; } = new();
    public List<OpprettVedtaksvirkningDto> Virkninger { get; set; } = new();
}

public class ForklaringsloggOppforingDto
{
    public Guid OppforingId { get; set; }
    public OppforingsType Type { get; set; }
    public Guid ReferanseId { get; set; }
}

public class ForklaringsloggDto
{
    public Guid LoggId { get; set; }
    public Guid VedtakId { get; set; }
    public List<ForklaringsloggOppforingDto> Oppforinger { get; set; } = new();
}

/// <summary>
/// Hydrert forklaring: vedtak + alle refererte faktum/vurdering/partsmedvirkning-rader
/// utfoldet. Det frosne øyeblikksbildet: kun det forklaringsloggen peker på. Fra v1.7.0
/// følger også vurderingstreet, oppløst referansedata og kryss-sak-referanser med, slik at
/// svaret kan vises uten flere oppslag.
/// </summary>
public class HydrertForklaringDto
{
    public VedtakDto Vedtak { get; set; } = null!;
    public ForklaringsloggDto Forklaringslogg { get; set; } = null!;
    public List<FaktumDto> Faktum { get; set; } = new();
    public List<VurderingDto> Vurderinger { get; set; } = new();
    public List<VurderingNodeDto> Vurderingstre { get; set; } = new();
    public List<PartsmedvirkningDto> Partsmedvirkninger { get; set; } = new();
    public List<VedtaksvirkningDto> Virkninger { get; set; } = new();

    /// <summary>Vurderinger i andre (frosne) saker som vurderinger her bygger på (regel 3.11). Følges ett nivå.</summary>
    public List<VurderingDto> RefererteVurderinger { get; set; } = new();

    /// <summary>Faktum som er referert fra vurderinger eller virkninger her, men ikke er oppført i loggen (typisk fra en annen sak).</summary>
    public List<FaktumDto> AndreFaktum { get; set; } = new();

    public ReferansedataDto Referansedata { get; set; } = new();
}
