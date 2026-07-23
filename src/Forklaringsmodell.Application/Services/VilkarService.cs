using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class VilkarService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettVilkarDto> _validator;

    public VilkarService(IForklaringsmodellRepository repository, IValidator<OpprettVilkarDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<VilkarDto>> ListAsync(CancellationToken ct = default)
    {
        var vilkar = await _repository.GetVilkarListAsync(ct);
        var dtos = new List<VilkarDto>();
        foreach (var v in vilkar)
        {
            dtos.Add(await ToDtoAsync(v, ct));
        }
        return dtos;
    }

    public async Task<VilkarDto> OpprettAsync(OpprettVilkarDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var rettskilder = dto.RettskildeIder.Count > 0
            ? await _repository.GetRettskilderByIderAsync(dto.RettskildeIder, ct)
            : new List<Rettskilde>();
        var manglendeRettskilder = dto.RettskildeIder.Except(rettskilder.Select(r => r.RettskildeId)).ToList();
        if (manglendeRettskilder.Count > 0)
        {
            throw new NotFoundException($"Rettskilde {string.Join(", ", manglendeRettskilder)} finnes ikke.");
        }

        if (dto.RegelId.HasValue)
        {
            _ = await _repository.GetRegelAsync(dto.RegelId.Value, ct)
                ?? throw new NotFoundException($"Regel {dto.RegelId} finnes ikke.");
        }

        var vilkar = new Vilkar
        {
            VilkarId = Guid.NewGuid(),
            Navn = dto.Navn,
            Kode = dto.Kode,
            Kodeverk = dto.Kodeverk,
            Type = dto.Type,
            Grunnlagstype = dto.Grunnlagstype!.Value, // validator sikrer NotNull før vi kommer hit
            Fastsettelsesmate = dto.Fastsettelsesmate,
            StandardTekst = dto.StandardTekst,
            RegelId = dto.RegelId,
            CpsvTjenesteReferanse = dto.CpsvTjenesteReferanse
        };

        foreach (var rettskilde in rettskilder)
        {
            vilkar.VilkarRettskilde.Add(new VilkarRettskilde { VilkarId = vilkar.VilkarId, RettskildeId = rettskilde.RettskildeId });
        }

        await _repository.AddVilkarAsync(vilkar, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(vilkar, ct);
    }

    /// <summary>
    /// Regel 3.12: en Vilkar-rad som er referert av minst én Vedtaksvirkning skal ikke
    /// overskrives — samme append-only-mønster som Regel (regel 3.4). Bevisst ikke
    /// eksponert som kontroller-metode i dag, men servicen håndhever regelen uavhengig.
    /// </summary>
    public async Task SjekkKanEndreAsync(Guid vilkarId, CancellationToken ct = default)
    {
        var erReferert = await _repository.ErVilkarReferertAsync(vilkarId, ct);
        if (erReferert)
        {
            throw new AppendOnlyViolationException(
                $"Vilkar {vilkarId} er referert av minst én Vedtaksvirkning og kan ikke endres.");
        }
    }

    private async Task<VilkarDto> ToDtoAsync(Vilkar vilkar, CancellationToken ct) => new()
    {
        VilkarId = vilkar.VilkarId,
        Navn = vilkar.Navn,
        Kode = vilkar.Kode,
        Kodeverk = vilkar.Kodeverk,
        Type = vilkar.Type,
        Grunnlagstype = vilkar.Grunnlagstype,
        Fastsettelsesmate = vilkar.Fastsettelsesmate,
        StandardTekst = vilkar.StandardTekst,
        RettskildeIder = vilkar.VilkarRettskilde.Select(vr => vr.RettskildeId).ToList(),
        RegelId = vilkar.RegelId,
        CpsvTjenesteReferanse = vilkar.CpsvTjenesteReferanse,
        ErLaast = await _repository.ErVilkarReferertAsync(vilkar.VilkarId, ct)
    };
}
