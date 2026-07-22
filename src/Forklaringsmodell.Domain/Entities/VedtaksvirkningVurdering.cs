namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet: hvilke(n) Vurdering(er) fastsatte en Vedtaksvirkning.</summary>
public class VedtaksvirkningVurdering
{
    public Guid VirkningId { get; set; }
    public Guid VurderingId { get; set; }

    public Vedtaksvirkning? Vedtaksvirkning { get; set; }
    public Vurdering? Vurdering { get; set; }
}
