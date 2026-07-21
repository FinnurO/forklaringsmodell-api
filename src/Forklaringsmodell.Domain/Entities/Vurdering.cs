using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Vurdering
{
    public Guid VurderingId { get; set; }
    public Guid SakId { get; set; }
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }        // faktisk brukt type (kan avvike fra Regel.Type ved eskalering)
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }          // 0.0-1.0, kun relevant for GenerativKI
    public bool Eskalert { get; set; }
    public string? Hovedhensyn { get; set; }         // obligatorisk når Type == Skjonn
    public string? ForkastedeUtfall { get; set; }    // kontrastiv forklaring for skjønn

    /// <summary>
    /// Skrivebeskyttet fordi denne raden allerede er referert av en ForklaringsloggOppforing.
    /// Beregnet felt, ikke lagret i databasen.
    /// </summary>
    public bool ErLaast { get; set; }

    public Sak? Sak { get; set; }
    public Regel? Regel { get; set; }
    public ICollection<VurderingFaktum> VurderingFaktum { get; set; } = new List<VurderingFaktum>();
    public ICollection<VurderingRettskilde> VurderingRettskilde { get; set; } = new List<VurderingRettskilde>();
}
