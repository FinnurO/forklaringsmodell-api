using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>Regel 3.10: GyldigTil skal være null når Varighet == Varig; RapporteringsFrekvens kun ved Type == Plikt.</summary>
public class VedtaksvirkningValidatorTests
{
    private readonly OpprettVedtaksvirkningDtoValidator _validator = new();

    [Fact]
    public async Task Varig_med_gyldigtil_gir_valideringsfeil()
    {
        var dto = new OpprettVedtaksvirkningDto
        {
            Type = VirkningType.Tillatelse,
            Beskrivelse = "Test",
            Varighet = VarighetsType.Varig,
            GyldigTil = DateTimeOffset.UtcNow
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVedtaksvirkningDto.GyldigTil));
    }

    [Fact]
    public async Task Varig_uten_gyldigtil_er_gyldig()
    {
        var dto = new OpprettVedtaksvirkningDto
        {
            Type = VirkningType.Tillatelse,
            Beskrivelse = "Test",
            Varighet = VarighetsType.Varig
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RapporteringsFrekvens_uten_plikt_gir_valideringsfeil()
    {
        var dto = new OpprettVedtaksvirkningDto
        {
            Type = VirkningType.Tillatelse,
            Beskrivelse = "Test",
            Varighet = VarighetsType.Tidsbegrenset,
            RapporteringsFrekvens = "Kvartalsvis"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVedtaksvirkningDto.RapporteringsFrekvens));
    }

    [Fact]
    public async Task RapporteringsFrekvens_med_plikt_er_gyldig()
    {
        var dto = new OpprettVedtaksvirkningDto
        {
            Type = VirkningType.Plikt,
            Beskrivelse = "Test",
            Varighet = VarighetsType.LopendeInntilVilkarBrister,
            RapporteringsFrekvens = "Kvartalsvis"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }
}
