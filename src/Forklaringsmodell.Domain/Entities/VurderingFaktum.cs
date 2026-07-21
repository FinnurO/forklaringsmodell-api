namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Join-entitet for mange-til-mange-relasjonen mellom Vurdering og Faktum
/// (spesifikasjonens "Vurdering.FaktumIder"-liste er en forenkling for API-svar;
/// EF Core-modellen krever en egen join-tabell).
/// </summary>
public class VurderingFaktum
{
    public Guid VurderingId { get; set; }
    public Guid FaktumId { get; set; }

    public Vurdering? Vurdering { get; set; }
    public Faktum? Faktum { get; set; }
}
