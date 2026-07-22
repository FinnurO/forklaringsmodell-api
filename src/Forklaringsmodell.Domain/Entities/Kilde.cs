using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Domain.Entities;

public class Kilde
{
    public Guid KildeId { get; set; }
    public string Navn { get; set; } = string.Empty;
    public KildeType Type { get; set; }
    public bool Autoritativ { get; set; }
    public string? CccevReferanse { get; set; } // valgfri IRI til cccev:Evidence eller cccev:Criterion

    /// <summary>
    /// Skrivebeskyttet fordi denne kilden allerede er brukt av minst ett Faktum (regel 3.8-analog
    /// append-only-mønster, se KildeService.SjekkKanEndreAsync). Beregnet felt, ikke lagret.
    /// </summary>
    public bool ErLaast { get; set; }

    public ICollection<Faktum> Faktum { get; set; } = new List<Faktum>();
    public ICollection<KildeRettskilde> KildeRettskilde { get; set; } = new List<KildeRettskilde>();
}
