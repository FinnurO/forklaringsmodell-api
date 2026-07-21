using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class SakService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettSakDto> _opprettValidator;
    private readonly IValidator<OppdaterSakDto> _oppdaterValidator;

    public SakService(
        IForklaringsmodellRepository repository,
        IValidator<OpprettSakDto> opprettValidator,
        IValidator<OppdaterSakDto> oppdaterValidator)
    {
        _repository = repository;
        _opprettValidator = opprettValidator;
        _oppdaterValidator = oppdaterValidator;
    }

    public async Task<List<SakDto>> ListAsync(CancellationToken ct = default)
    {
        var saker = await _repository.GetSakerAsync(ct);
        return saker.Select(ToDto).ToList();
    }

    public async Task<SakDto> GetAsync(Guid sakId, CancellationToken ct = default)
    {
        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        return ToDto(sak);
    }

    public async Task<SakDto> OpprettAsync(OpprettSakDto dto, CancellationToken ct = default)
    {
        var result = await _opprettValidator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var naa = DateTimeOffset.UtcNow;
        var sak = new Sak
        {
            SakId = Guid.NewGuid(),
            Tittel = dto.Tittel,
            Status = dto.Status,
            Opprettet = naa,
            SistEndret = naa,
            TjenesteReferanse = dto.TjenesteReferanse
        };

        await _repository.AddSakAsync(sak, ct);
        await _repository.SaveChangesAsync(ct);
        return ToDto(sak);
    }

    /// <summary>
    /// Regel 3.6: Sak er mutable helt til/selv etter Vedtak finnes (Sak låses ikke selv,
    /// bare de refererte Faktum/Vurdering/Partsmedvirkning/Regel-radene). Sak kan derfor
    /// alltid oppdateres (tittel/status), også etter at ett eller flere Vedtak er opprettet
    /// på den, slik at nye Vedtak (klage/omgjøring) kan følge senere.
    /// </summary>
    public async Task<SakDto> OppdaterAsync(Guid sakId, OppdaterSakDto dto, CancellationToken ct = default)
    {
        var result = await _oppdaterValidator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");

        sak.Tittel = dto.Tittel;
        sak.Status = dto.Status;
        sak.SistEndret = DateTimeOffset.UtcNow;

        await _repository.SaveChangesAsync(ct);
        return ToDto(sak);
    }

    private static SakDto ToDto(Sak sak) => new()
    {
        SakId = sak.SakId,
        Tittel = sak.Tittel,
        Status = sak.Status,
        Opprettet = sak.Opprettet,
        SistEndret = sak.SistEndret,
        TjenesteReferanse = sak.TjenesteReferanse
    };
}
