using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>
/// Regel 3.7 (Regel skal ha minst én tilknyttet Rettskilde) og regel 3.8 (Kilde skal ha
/// minst én tilknyttet Rettskilde før den kan brukes til å registrere Faktum).
/// </summary>
public class RettskildeValidatorTests
{
    [Fact]
    public async Task Kilde_uten_rettskilde_gir_valideringsfeil()
    {
        var validator = new OpprettKildeDtoValidator();
        var dto = new OpprettKildeDto
        {
            Navn = "Test-kilde",
            Type = KildeType.AnnenKilde,
            Autoritativ = false,
            RettskildeIder = new List<Guid>()
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettKildeDto.RettskildeIder));
    }

    [Fact]
    public async Task Kilde_med_rettskilde_er_gyldig()
    {
        var validator = new OpprettKildeDtoValidator();
        var dto = new OpprettKildeDto
        {
            Navn = "Test-kilde",
            Type = KildeType.AnnenKilde,
            Autoritativ = false,
            RettskildeIder = new List<Guid> { Guid.NewGuid() }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Regel_uten_rettskilde_gir_valideringsfeil()
    {
        var validator = new OpprettRegelDtoValidator();
        var dto = new OpprettRegelDto
        {
            Teknologi = "DMN",
            Type = VurderingsType.Deterministisk,
            RettskildeIder = new List<Guid>()
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OpprettRegelDto.RettskildeIder));
    }
}
