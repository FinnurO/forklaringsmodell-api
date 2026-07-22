using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Generell, gjenbrukbar referansetabell for vilkårsdefinisjoner (som Regel/Rettskilde/
/// Kilde) — kan romme alt fra et fast standardvilkår til et parametrisert eller
/// skjønnsbasert vilkår. Append-only når referert av minst én Vedtaksvirkning (regel 3.12).
/// </summary>
public class Vilkar
{
    public Guid VilkarId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public VirkningType Type { get; set; }
    public FastsettelsesmateType Fastsettelsesmate { get; set; }
    public string? StandardTekst { get; set; }
    public Guid? RegelId { get; set; }

    public Regel? Regel { get; set; }
    public ICollection<VilkarRettskilde> VilkarRettskilde { get; set; } = new List<VilkarRettskilde>();
    public ICollection<Vedtaksvirkning> Vedtaksvirkninger { get; set; } = new List<Vedtaksvirkning>();
}
