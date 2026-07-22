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

/// <summary>Hydrert forklaring: vedtak + alle refererte faktum/vurdering/partsmedvirkning-rader utfoldet.</summary>
public class HydrertForklaringDto
{
    public VedtakDto Vedtak { get; set; } = null!;
    public ForklaringsloggDto Forklaringslogg { get; set; } = null!;
    public List<FaktumDto> Faktum { get; set; } = new();
    public List<VurderingDto> Vurderinger { get; set; } = new();
    public List<PartsmedvirkningDto> Partsmedvirkninger { get; set; } = new();
    public List<VedtaksvirkningDto> Virkninger { get; set; } = new();
}
