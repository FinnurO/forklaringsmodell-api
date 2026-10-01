namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Én språkvariant av en FlerspraakligTekst. SpraakKode er bevisst en fri streng
/// (f.eks. "nb", "nn", "en"), ikke en enum — nye språk krever da ingen skjemaendring,
/// bare nye rader (regel 3.17).
/// </summary>
public class TekstVariant
{
    public Guid FlerspraakligTekstId { get; set; }
    public string SpraakKode { get; set; } = string.Empty;
    public string Verdi { get; set; } = string.Empty;

    public FlerspraakligTekst? FlerspraakligTekst { get; set; }
}
