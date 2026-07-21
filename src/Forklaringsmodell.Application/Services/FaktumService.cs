using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class FaktumService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettFaktumDto> _opprettValidator;
    private readonly IValidator<TransformerFaktumDto> _transformerValidator;

    public FaktumService(
        IForklaringsmodellRepository repository,
        IValidator<OpprettFaktumDto> opprettValidator,
        IValidator<TransformerFaktumDto> transformerValidator)
    {
        _repository = repository;
        _opprettValidator = opprettValidator;
        _transformerValidator = transformerValidator;
    }

    public async Task<List<FaktumDto>> ListForSakAsync(Guid sakId, CancellationToken ct = default)
    {
        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        var faktum = await _repository.GetFaktumForSakAsync(sak.SakId, ct);
        var dtos = new List<FaktumDto>();
        foreach (var f in faktum)
        {
            dtos.Add(await ToDtoAsync(f, ct));
        }
        return dtos;
    }

    public async Task<FaktumDto> GetAsync(Guid faktumId, CancellationToken ct = default)
    {
        var faktum = await _repository.GetFaktumAsync(faktumId, ct) ?? throw new NotFoundException($"Faktum {faktumId} finnes ikke.");
        return await ToDtoAsync(faktum, ct);
    }

    public async Task<FaktumDto> OpprettAsync(Guid sakId, OpprettFaktumDto dto, CancellationToken ct = default)
    {
        var result = await _opprettValidator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        _ = await _repository.GetKildeAsync(dto.KildeId, ct) ?? throw new NotFoundException($"Kilde {dto.KildeId} finnes ikke.");

        var rettskilder = dto.RettskildeIder.Count > 0
            ? await _repository.GetRettskilderByIderAsync(dto.RettskildeIder, ct)
            : new List<Rettskilde>();
        var manglendeRettskilder = dto.RettskildeIder.Except(rettskilder.Select(r => r.RettskildeId)).ToList();
        if (manglendeRettskilder.Count > 0)
        {
            throw new NotFoundException($"Rettskilde {string.Join(", ", manglendeRettskilder)} finnes ikke.");
        }

        var faktum = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sak.SakId,
            KildeId = dto.KildeId,
            Type = dto.Type,
            Struktur = dto.Struktur,
            Verdi = dto.Verdi,
            InnhentetTidspunkt = dto.InnhentetTidspunkt ?? DateTimeOffset.UtcNow
        };

        foreach (var rettskilde in rettskilder)
        {
            faktum.FaktumRettskilde.Add(new FaktumRettskilde { FaktumId = faktum.FaktumId, RettskildeId = rettskilde.RettskildeId });
        }

        await _repository.AddFaktumAsync(faktum, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(faktum, ct);
    }

    /// <summary>
    /// Oppretter et nytt subsumert Faktum avledet fra et rått faktum (setter
    /// AvledetFraFaktumId automatisk). Dette er den eneste "korrigeringsveien" for et
    /// faktum som allerede er låst (regel 3.1) — i stedet for å overskrive originalen
    /// legges en ny rad til og kjeden bevares via AvledetFraFaktumId.
    /// </summary>
    public async Task<FaktumDto> TransformerAsync(Guid faktumId, TransformerFaktumDto dto, CancellationToken ct = default)
    {
        var result = await _transformerValidator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var kilde = await _repository.GetFaktumAsync(faktumId, ct) ?? throw new NotFoundException($"Faktum {faktumId} finnes ikke.");

        var nyttFaktum = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = kilde.SakId,
            KildeId = dto.KildeId ?? kilde.KildeId,
            Type = FaktumType.Subsumert,
            Struktur = dto.Struktur,
            Verdi = dto.Verdi,
            AvledetFraFaktumId = kilde.FaktumId,
            InnhentetTidspunkt = DateTimeOffset.UtcNow
        };

        await _repository.AddFaktumAsync(nyttFaktum, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(nyttFaktum, ct);
    }

    /// <summary>
    /// Regel 3.1: dersom et Faktum allerede er referert av en ForklaringsloggOppforing
    /// (dvs. inngår i et frosset Vedtak), skal endring avvises. Kalles fra ethvert
    /// fremtidig "oppdater faktum"-endepunkt/verb (bevisst ikke eksponert som kontroller-
    /// metode i dag, men servicen håndhever regelen uavhengig av om et slikt endepunkt
    /// legges til senere).
    /// </summary>
    public async Task SjekkKanEndreAsync(Guid faktumId, CancellationToken ct = default)
    {
        var erReferert = await _repository.ErFaktumReferertAsync(faktumId, ct);
        if (erReferert)
        {
            throw new AppendOnlyViolationException(
                $"Faktum {faktumId} er referert av en ForklaringsloggOppforing og kan ikke endres. " +
                "Opprett et nytt (subsumert) faktum via /api/faktum/{id}/transformer i stedet.");
        }
    }

    private async Task<FaktumDto> ToDtoAsync(Faktum faktum, CancellationToken ct) => new()
    {
        FaktumId = faktum.FaktumId,
        SakId = faktum.SakId,
        KildeId = faktum.KildeId,
        Type = faktum.Type,
        Struktur = faktum.Struktur,
        Verdi = faktum.Verdi,
        AvledetFraFaktumId = faktum.AvledetFraFaktumId,
        InnhentetTidspunkt = faktum.InnhentetTidspunkt,
        RettskildeIder = faktum.FaktumRettskilde.Select(fr => fr.RettskildeId).ToList(),
        ErLaast = await _repository.ErFaktumReferertAsync(faktum.FaktumId, ct)
    };
}
