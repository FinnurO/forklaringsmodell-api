namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet for mange-til-mange mellom Regel og Rettskilde (regel 3.7).</summary>
public class RegelRettskilde
{
    public Guid RegelId { get; set; }
    public Guid RettskildeId { get; set; }

    public Regel? Regel { get; set; }
    public Rettskilde? Rettskilde { get; set; }
}
