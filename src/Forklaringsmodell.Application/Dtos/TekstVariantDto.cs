namespace Forklaringsmodell.Application.Dtos;

/// <summary>Én språkvariant av et flerspråklig forklaringstekst-felt, se regel 3.17.</summary>
public class TekstVariantDto
{
    public string SpraakKode { get; set; } = string.Empty;
    public string Verdi { get; set; } = string.Empty;
}
