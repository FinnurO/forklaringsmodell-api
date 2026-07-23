using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Tests.Unit;

public class VilkarValidatorTests
{
    private readonly OpprettVilkarDtoValidator _validator = new();

    [Fact]
    public async Task Vilkar_uten_navn_gir_valideringsfeil()
    {
        var dto = new OpprettVilkarDto
        {
            Navn = "",
            Type = VirkningType.Tillatelse,
            Fastsettelsesmate = FastsettelsesmateType.Statisk
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVilkarDto.Navn));
    }

    [Fact]
    public async Task Vilkar_med_navn_er_gyldig()
    {
        var dto = new OpprettVilkarDto
        {
            Navn = "Skjenketid gruppe 3 innendørs",
            Type = VirkningType.Tillatelse,
            Grunnlagstype = GrunnlagsType.InternPraksis,
            Fastsettelsesmate = FastsettelsesmateType.Parametrisert
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Vilkar_uten_grunnlagstype_gir_valideringsfeil()
    {
        var dto = new OpprettVilkarDto
        {
            Navn = "Test-vilkår",
            Type = VirkningType.Tillatelse,
            Fastsettelsesmate = FastsettelsesmateType.Statisk,
            Grunnlagstype = null
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVilkarDto.Grunnlagstype));
    }

    /// <summary>Regel 3.15: Grunnlagstype == Rettslig krever minst én RettskildeIder.</summary>
    [Fact]
    public async Task Rettslig_vilkar_uten_rettskilde_gir_valideringsfeil()
    {
        var dto = new OpprettVilkarDto
        {
            Navn = "Test-vilkår",
            Type = VirkningType.Tillatelse,
            Fastsettelsesmate = FastsettelsesmateType.Statisk,
            Grunnlagstype = GrunnlagsType.Rettslig,
            RettskildeIder = new List<Guid>()
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettVilkarDto.RettskildeIder));
    }

    [Theory]
    [InlineData(GrunnlagsType.InternPraksis)]
    [InlineData(GrunnlagsType.Datakvalitet)]
    public async Task InternPraksis_og_Datakvalitet_krever_ikke_rettskilde(GrunnlagsType grunnlagstype)
    {
        var dto = new OpprettVilkarDto
        {
            Navn = "Test-vilkår",
            Type = VirkningType.Tillatelse,
            Fastsettelsesmate = FastsettelsesmateType.Statisk,
            Grunnlagstype = grunnlagstype,
            RettskildeIder = new List<Guid>()
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }
}
