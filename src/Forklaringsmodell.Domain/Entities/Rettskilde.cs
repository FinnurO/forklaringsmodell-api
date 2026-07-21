using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Rettskilde
{
    public Guid RettskildeId { get; set; }
    public RettskildeType Type { get; set; }
    public string Henvisning { get; set; } = string.Empty; // f.eks. "folketrygdloven § 4-5", "NOU 2019:5", "Rt-2015-1234"
    public DateTimeOffset? VersjonDato { get; set; }        // kun meningsfullt for Lov/Forskrift
    public string? EliReferanse { get; set; }                // kun meningsfullt for Lov/Forskrift

    public ICollection<RegelRettskilde> RegelRettskilde { get; set; } = new List<RegelRettskilde>();
    public ICollection<KildeRettskilde> KildeRettskilde { get; set; } = new List<KildeRettskilde>();
    public ICollection<FaktumRettskilde> FaktumRettskilde { get; set; } = new List<FaktumRettskilde>();
    public ICollection<VurderingRettskilde> VurderingRettskilde { get; set; } = new List<VurderingRettskilde>();
}
