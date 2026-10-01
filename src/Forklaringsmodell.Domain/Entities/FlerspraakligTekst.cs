namespace Forklaringsmodell.Domain.Entities;

/// <summary>
/// Gjenbrukbar beholder for tekst på flere språk (regel 3.17). Brukes av de
/// forklaringstekst-feltene som faktisk inngår i begrunnelsen overfor en part
/// (Vilkar.StandardTekst, Vurdering.Hovedhensyn/ForkastedeUtfall,
/// Vedtaksvirkning.Beskrivelse/LopendeVilkar) — ikke interne/tekniske felt.
/// </summary>
public class FlerspraakligTekst
{
    public Guid FlerspraakligTekstId { get; set; }

    public ICollection<TekstVariant> Varianter { get; set; } = new List<TekstVariant>();
}
