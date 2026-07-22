namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet: sporbarhet fra en Vedtaksvirkning til Faktum den bygger på (f.eks. beløpsberegning).</summary>
public class VedtaksvirkningFaktum
{
    public Guid VirkningId { get; set; }
    public Guid FaktumId { get; set; }

    public Vedtaksvirkning? Vedtaksvirkning { get; set; }
    public Faktum? Faktum { get; set; }
}
