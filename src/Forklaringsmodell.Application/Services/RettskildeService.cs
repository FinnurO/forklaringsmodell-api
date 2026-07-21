using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class RettskildeService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettRettskildeDto> _validator;

    public RettskildeService(IForklaringsmodellRepository repository, IValidator<OpprettRettskildeDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<RettskildeDto>> ListAsync(CancellationToken ct = default)
    {
        var rettskilder = await _repository.GetRettskilderAsync(ct);
        return rettskilder.Select(ToDto).ToList();
    }

    public async Task<RettskildeDto> OpprettAsync(OpprettRettskildeDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var rettskilde = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Type = dto.Type,
            Henvisning = dto.Henvisning,
            VersjonDato = dto.VersjonDato,
            EliReferanse = dto.EliReferanse
        };

        await _repository.AddRettskildeAsync(rettskilde, ct);
        await _repository.SaveChangesAsync(ct);
        return ToDto(rettskilde);
    }

    private static RettskildeDto ToDto(Rettskilde rettskilde) => new()
    {
        RettskildeId = rettskilde.RettskildeId,
        Type = rettskilde.Type,
        Henvisning = rettskilde.Henvisning,
        VersjonDato = rettskilde.VersjonDato,
        EliReferanse = rettskilde.EliReferanse
    };
}
