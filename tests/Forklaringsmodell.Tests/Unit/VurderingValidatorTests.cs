using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Tests.Unit;

public class VurderingValidatorTests
{
    private readonly OpprettVurderingDtoValidator _validator = new();

    [Fact]
    public async Task Skjonn_uten_hovedhensyn_gir_valideringsfeil()
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Hovedhensyn = null
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVurderingDto.Hovedhensyn));
    }

    [Fact]
    public async Task Skjonn_med_tom_streng_hovedhensyn_gir_valideringsfeil()
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Hovedhensyn = ""
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Skjonn_med_hovedhensyn_er_gyldig()
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Skjonn,
            Hovedhensyn = "Dokumentert nedbemanning"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task Konfidens_utenfor_0_til_1_gir_valideringsfeil(double konfidens)
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.GenerativKI,
            Konfidens = (decimal)konfidens
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVurderingDto.Konfidens));
    }

    [Fact]
    public async Task Konfidens_satt_paa_deterministisk_vurdering_gir_valideringsfeil()
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.Deterministisk,
            Konfidens = 0.9m
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task GenerativKI_med_gyldig_konfidens_er_gyldig()
    {
        var dto = new OpprettVurderingDto
        {
            RegelId = Guid.NewGuid(),
            Type = VurderingsType.GenerativKI,
            Konfidens = 0.62m
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }
}
