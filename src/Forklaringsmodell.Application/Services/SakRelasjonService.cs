using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

/// <summary>
/// Regel 3.11: kobler en ny/oppfølgende Sak til en relatert Sak, uten å modifisere den
/// relaterte saken — ingen prosessmotor eller tilstandsmaskin, bare en referanse.
/// </summary>
public class SakRelasjonService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettSakRelasjonDto> _validator;

    public SakRelasjonService(IForklaringsmodellRepository repository, IValidator<OpprettSakRelasjonDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<SakRelasjonDto>> ListForSakAsync(Guid sakId, CancellationToken ct = default)
    {
        _ = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        var relasjoner = await _repository.GetSakRelasjonerForSakAsync(sakId, ct);
        return relasjoner.Select(ToDto).ToList();
    }

    public async Task<SakRelasjonDto> OpprettAsync(Guid sakId, OpprettSakRelasjonDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        _ = await _repository.GetSakAsync(dto.RelatertSakId, ct) ?? throw new NotFoundException($"Sak {dto.RelatertSakId} finnes ikke.");

        var relasjon = new SakRelasjon
        {
            RelasjonId = Guid.NewGuid(),
            SakId = sak.SakId,
            RelatertSakId = dto.RelatertSakId,
            Type = dto.Type
        };

        await _repository.AddSakRelasjonAsync(relasjon, ct);
        await _repository.SaveChangesAsync(ct);
        return ToDto(relasjon);
    }

    private static SakRelasjonDto ToDto(SakRelasjon relasjon) => new()
    {
        RelasjonId = relasjon.RelasjonId,
        SakId = relasjon.SakId,
        RelatertSakId = relasjon.RelatertSakId,
        Type = relasjon.Type
    };
}
