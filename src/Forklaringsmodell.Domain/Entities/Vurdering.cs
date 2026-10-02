using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Vurdering
{
    public Guid VurderingId { get; set; }
    public Guid SakId { get; set; }
    public Guid RegelId { get; set; }
    public VurderingsType Type { get; set; }        // faktisk brukt type (kan avvike fra Regel.Type ved eskalering)
    public UtfallType Utfall { get; set; }          // Oppfylt/IkkeOppfylt/Uaktuelt/IkkeVurdert/Uavklart, se regel 3.14
    public string? Beregningsspor { get; set; }
    public decimal? Konfidens { get; set; }          // 0.0-1.0, kun relevant for GenerativKI
    public bool Eskalert { get; set; }
    public Guid? HovedhensynTekstId { get; set; }       // obligatorisk når Type == Skjonn
    public Guid? ForkastedeUtfallTekstId { get; set; }  // kontrastiv forklaring for skjønn

    /// <summary>Hvilket katalogvilkår denne vurderingen gjelder — ren sporbarhetsmerking, ingen håndhevet struktur, se regel 3.18.</summary>
    public Guid? VilkarId { get; set; }

    /// <summary>
    /// Intra-sak foreldre-vurdering: lar en vurdering være en delvurdering av et mer
    /// overordnet vilkår, i samme (fortsatt åpne) sak — i motsetning til
    /// RefererteVurderinger, som kun peker til frosne rader i andre saker (regel 3.11).
    /// Gjengir trofast hvilken struktur det kallende systemet faktisk valgte, uten å
    /// håndheve eller forutsette noen fast mal, se regel 3.18.
    /// </summary>
    public Guid? ForelderVurderingId { get; set; }

    /// <summary>
    /// Skrivebeskyttet fordi denne raden allerede er referert av en ForklaringsloggOppforing.
    /// Beregnet felt, ikke lagret i databasen.
    /// </summary>
    public bool ErLaast { get; set; }

    public Sak? Sak { get; set; }
    public Regel? Regel { get; set; }
    public Vilkar? Vilkar { get; set; }
    public FlerspraakligTekst? HovedhensynTekst { get; set; }
    public FlerspraakligTekst? ForkastedeUtfallTekst { get; set; }
    public Vurdering? ForelderVurdering { get; set; }
    public ICollection<Vurdering> Delvurderinger { get; set; } = new List<Vurdering>();
    public ICollection<VurderingFaktum> VurderingFaktum { get; set; } = new List<VurderingFaktum>();
    public ICollection<VurderingRettskilde> VurderingRettskilde { get; set; } = new List<VurderingRettskilde>();

    /// <summary>Utgående referanser: vurderinger fra andre (frosne) saker denne bygger på (regel 3.11).</summary>
    public ICollection<VurderingReferanse> RefererteVurderinger { get; set; } = new List<VurderingReferanse>();
}
