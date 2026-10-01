using Forklaringsmodell.Application.Dtos;
using FluentValidation;

namespace Forklaringsmodell.Application.Validators;

/// <summary>Validerer én språkvariant. Gjenbrukt av alle fem flerspråklige forklaringstekst-felt (regel 3.17).</summary>
public class TekstVariantDtoValidator : AbstractValidator<TekstVariantDto>
{
    public TekstVariantDtoValidator()
    {
        RuleFor(x => x.SpraakKode).NotEmpty();
        RuleFor(x => x.Verdi).NotEmpty();
    }

    /// <summary>
    /// Regel 3.17: maks én TekstVariant per språk. SpraakKode er bevisst ikke en enum
    /// (fritt felt, se Domain), så unikheten må sjekkes i applikasjonslaget i stedet for
    /// via databasens egen enum-begrensning.
    /// </summary>
    public static bool IngenDuplikateSpraakkoder(IEnumerable<TekstVariantDto> varianter) =>
        varianter.Select(v => v.SpraakKode).Distinct(StringComparer.OrdinalIgnoreCase).Count() == varianter.Count();
}
