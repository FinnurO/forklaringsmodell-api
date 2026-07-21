using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class PartsmedvirkningService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettPartsmedvirkningDto> _validator;

    public PartsmedvirkningService(IForklaringsmodellRepository repository, IValidator<OpprettPartsmedvirkningDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<PartsmedvirkningDto>> ListForSakAsync(Guid sakId, CancellationToken ct = default)
    {
        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        var rader = await _repository.GetPartsmedvirkningerForSakAsync(sak.SakId, ct);
        return rader.Select(ToDto).ToList();
    }

    public async Task<PartsmedvirkningDto> OpprettAsync(Guid sakId, OpprettPartsmedvirkningDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");

        var partsmedvirkning = new Partsmedvirkning
        {
            MedvirkningId = Guid.NewGuid(),
            SakId = sak.SakId,
            Type = dto.Type,
            Tidspunkt = dto.Tidspunkt ?? DateTimeOffset.UtcNow,
            Innhold = dto.Innhold
        };

        await _repository.AddPartsmedvirkningAsync(partsmedvirkning, ct);
        await _repository.SaveChangesAsync(ct);
        return ToDto(partsmedvirkning);
    }

    private static PartsmedvirkningDto ToDto(Partsmedvirkning p) => new()
    {
        MedvirkningId = p.MedvirkningId,
        SakId = p.SakId,
        Type = p.Type,
        Tidspunkt = p.Tidspunkt,
        Innhold = p.Innhold
    };
}
