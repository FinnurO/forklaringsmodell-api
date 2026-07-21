using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Regel
{
    public Guid RegelId { get; set; }
    public string Teknologi { get; set; } = string.Empty; // f.eks. "DMN", "Python", "LLM-prompt v3"
    public VurderingsType Type { get; set; }               // regelens konfigurerte type
    public string? CpsvRuleReferanse { get; set; }          // valgfri URI til CPSV-AP Rule

    /// <summary>
    /// Skrivebeskyttet fordi denne raden allerede er referert av minst én Vurdering (regel 3.4).
    /// Beregnet felt, ikke lagret i databasen.
    /// </summary>
    public bool ErLaast { get; set; }

    public ICollection<RegelRettskilde> RegelRettskilde { get; set; } = new List<RegelRettskilde>();
    public ICollection<Vurdering> Vurderinger { get; set; } = new List<Vurdering>();
}
