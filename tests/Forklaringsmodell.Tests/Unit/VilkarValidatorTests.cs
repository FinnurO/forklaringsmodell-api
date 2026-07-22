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
            Fastsettelsesmate = FastsettelsesmateType.Parametrisert
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }
}
