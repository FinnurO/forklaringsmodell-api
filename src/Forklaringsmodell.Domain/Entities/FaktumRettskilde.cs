namespace Forklaringsmodell.Domain.Entities;

/// <summary>Join-entitet for mange-til-mange mellom Faktum og Rettskilde (regel 3.8, valgfri tilleggshjemmel).</summary>
public class FaktumRettskilde
{
    public Guid FaktumId { get; set; }
    public Guid RettskildeId { get; set; }

    public Faktum? Faktum { get; set; }
    public Rettskilde? Rettskilde { get; set; }
}
