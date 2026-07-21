namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet for mange-til-mange mellom Vurdering og Rettskilde (regel 3.7, saksspesifikke kilder).</summary>
public class VurderingRettskilde
{
    public Guid VurderingId { get; set; }
    public Guid RettskildeId { get; set; }

    public Vurdering? Vurdering { get; set; }
    public Rettskilde? Rettskilde { get; set; }
}
