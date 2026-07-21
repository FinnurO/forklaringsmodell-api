namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet for mange-til-mange mellom Kilde og Rettskilde (regel 3.8).</summary>
public class KildeRettskilde
{
    public Guid KildeId { get; set; }
    public Guid RettskildeId { get; set; }

    public Kilde? Kilde { get; set; }
    public Rettskilde? Rettskilde { get; set; }
}
