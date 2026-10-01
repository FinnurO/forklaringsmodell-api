using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Domain.Entities;

namespace Forklaringsmodell.Application.Mapping;

/// <summary>
/// Delt konvertering mellom <see cref="FlerspraakligTekst"/> og den flate
/// <see cref="TekstVariantDto"/>-listen klienten sender/mottar (regel 3.17). Brukt av
/// VilkarService, VurderingService og VedtakService — de fem forklaringstekst-feltene
/// følger samme mønster.
/// </summary>
public static class FlerspraakligTekstMapper
{
    /// <summary>Bygger en ny FlerspraakligTekst fra en variantliste, eller null hvis listen er tom (valgfritt felt).</summary>
    public static FlerspraakligTekst? TilEntitet(IReadOnlyCollection<TekstVariantDto> varianter)
    {
        if (varianter.Count == 0)
        {
            return null;
        }

        var tekst = new FlerspraakligTekst { FlerspraakligTekstId = Guid.NewGuid() };
        foreach (var variant in varianter)
        {
            tekst.Varianter.Add(new TekstVariant
            {
                FlerspraakligTekstId = tekst.FlerspraakligTekstId,
                SpraakKode = variant.SpraakKode,
                Verdi = variant.Verdi
            });
        }

        return tekst;
    }

    public static List<TekstVariantDto> TilDto(FlerspraakligTekst? tekst) =>
        tekst?.Varianter.Select(v => new TekstVariantDto { SpraakKode = v.SpraakKode, Verdi = v.Verdi }).ToList() ?? new List<TekstVariantDto>();
}
