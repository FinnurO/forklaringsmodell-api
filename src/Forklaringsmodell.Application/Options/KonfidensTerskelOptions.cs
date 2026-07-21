namespace Forklaringsmodell.Application.Options;

/// <summary>
/// Pragmatisk løsning for spesifikasjonens regel 3.3: "Terskel for eskalering er ikke en
/// hardkodet konstant i API-et, men bør leses fra konfigurasjon per Regel."
///
/// Bindes fra appsettings-seksjonen "KonfidensTerskler":
/// {
///   "KonfidensTerskler": {
///     "Default": 0.7,
///     "PerRegel": { "&lt;regelId-guid&gt;": 0.8 }
///   }
/// }
///
/// Oppslag: hvis Regel.RegelId finnes som nøkkel i PerRegel, brukes den verdien, ellers
/// brukes Default. Se VurderingService for hvordan terskelen brukes til å avgjøre om en
/// GenerativKI-vurdering med lav konfidens bør eskaleres.
/// </summary>
public class KonfidensTerskelOptions
{
    public const string SectionName = "KonfidensTerskler";

    public decimal Default { get; set; } = 0.7m;
    public Dictionary<string, decimal> PerRegel { get; set; } = new();

    public decimal ForRegel(Guid regelId)
    {
        return PerRegel.TryGetValue(regelId.ToString(), out var terskel) ? terskel : Default;
    }
}
