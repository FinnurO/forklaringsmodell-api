namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Selvrefererende join-entitet for Vurdering.RefererteVurderingIder (regel 3.11): en
/// vurdering (VurderingId) bygger på en allerede frosset vurdering fra en annen sak
/// (RefererteVurderingId). Alltid skrivebeskyttet i den refererte retningen.
/// </summary>
public class VurderingReferanse
{
    public Guid VurderingId { get; set; }
    public Guid RefererteVurderingId { get; set; }

    public Vurdering? Vurdering { get; set; }
    public Vurdering? RefererteVurdering { get; set; }
}
