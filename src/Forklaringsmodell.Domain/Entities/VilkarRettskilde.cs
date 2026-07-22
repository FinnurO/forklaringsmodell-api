namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet for mange-til-mange mellom Vilkar og Rettskilde (regel 3.7-mønster).</summary>
public class VilkarRettskilde
{
    public Guid VilkarId { get; set; }
    public Guid RettskildeId { get; set; }

    public Vilkar? Vilkar { get; set; }
    public Rettskilde? Rettskilde { get; set; }
}
