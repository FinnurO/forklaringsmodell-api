using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>Regel 3.17: flerspråklig forklaringstekst — ingen duplikate språkkoder, og hver variant må ha både språkkode og verdi.</summary>
public class TekstVariantValidatorTests
{
    [Fact]
    public async Task Duplikat_spraakkode_i_hovedhensyn_gir_valideringsfeil()
    {
        var validator = new OpprettVurderingDtoValidator();
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Utfall = UtfallType.Oppfylt,
            Hovedhensyn = new List<TekstVariantDto>
            {
                new() { SpraakKode = "nb", Verdi = "Første" },
                new() { SpraakKode = "NB", Verdi = "Andre" }
            }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVurderingDto.Hovedhensyn));
    }

    [Fact]
    public async Task Flere_ulike_spraakkoder_i_hovedhensyn_er_gyldig()
    {
        var validator = new OpprettVurderingDtoValidator();
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Utfall = UtfallType.Oppfylt,
            Hovedhensyn = new List<TekstVariantDto>
            {
                new() { SpraakKode = "nb", Verdi = "Bokmål" },
                new() { SpraakKode = "nn", Verdi = "Nynorsk" }
            }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Variant_uten_verdi_gir_valideringsfeil()
    {
        var validator = new OpprettVurderingDtoValidator();
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Utfall = UtfallType.Oppfylt,
            Hovedhensyn = new List<TekstVariantDto>
            {
                new() { SpraakKode = "nb", Verdi = "" }
            }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void IngenDuplikateSpraakkoder_er_case_insensitiv()
    {
        var varianter = new List<TekstVariantDto>
        {
            new() { SpraakKode = "nb", Verdi = "A" },
            new() { SpraakKode = "Nb", Verdi = "B" }
        };

        Assert.False(TekstVariantDtoValidator.IngenDuplikateSpraakkoder(varianter));
    }
}
