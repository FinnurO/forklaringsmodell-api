using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class KildeService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettKildeDto> _validator;

    public KildeService(IForklaringsmodellRepository repository, IValidator<OpprettKildeDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<KildeDto>> ListAsync(CancellationToken ct = default)
    {
        var kilder = await _repository.GetKilderAsync(ct);
        return kilder.Select(ToDto).ToList();
    }

    public async Task<KildeDto> OpprettAsync(OpprettKildeDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var kilde = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = dto.Navn,
            Type = dto.Type,
            Autoritativ = dto.Autoritativ
        };

        await _repository.AddKildeAsync(kilde, ct);
        await _repository.SaveChangesAsync(ct);
        return ToDto(kilde);
    }

    private static KildeDto ToDto(Kilde kilde) => new()
    {
        KildeId = kilde.KildeId,
        Navn = kilde.Navn,
        Type = kilde.Type,
        Autoritativ = kilde.Autoritativ
    };
}
