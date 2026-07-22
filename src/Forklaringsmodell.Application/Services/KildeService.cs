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
        var dtos = new List<KildeDto>();
        foreach (var kilde in kilder)
        {
            dtos.Add(await ToDtoAsync(kilde, ct));
        }
        return dtos;
    }

    public async Task<KildeDto> OpprettAsync(OpprettKildeDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var rettskilder = await _repository.GetRettskilderByIderAsync(dto.RettskildeIder, ct);
        var manglende = dto.RettskildeIder.Except(rettskilder.Select(r => r.RettskildeId)).ToList();
        if (manglende.Count > 0)
        {
            throw new NotFoundException($"Rettskilde {string.Join(", ", manglende)} finnes ikke.");
        }

        var kilde = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = dto.Navn,
            Type = dto.Type,
            Autoritativ = dto.Autoritativ,
            CccevReferanse = dto.CccevReferanse
        };

        foreach (var rettskilde in rettskilder)
        {
            kilde.KildeRettskilde.Add(new KildeRettskilde { KildeId = kilde.KildeId, RettskildeId = rettskilde.RettskildeId });
        }

        await _repository.AddKildeAsync(kilde, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(kilde, ct);
    }

    /// <summary>
    /// Append-only-analog for Kilde (spesifikasjonens punkt 5): dersom en Kilde allerede
    /// er brukt av minst ett Faktum, skal endring avvises. Se tilsvarende kommentar i
    /// FaktumService/VurderingService — bevisst ikke eksponert som kontroller-metode i
    /// dag, men servicen håndhever regelen uavhengig av om et slikt endepunkt legges til.
    /// </summary>
    public async Task SjekkKanEndreAsync(Guid kildeId, CancellationToken ct = default)
    {
        var erReferert = await _repository.ErKildeReferertAsync(kildeId, ct);
        if (erReferert)
        {
            throw new AppendOnlyViolationException(
                $"Kilde {kildeId} er brukt av minst ett Faktum og kan ikke endres.");
        }
    }

    private async Task<KildeDto> ToDtoAsync(Kilde kilde, CancellationToken ct) => new()
    {
        KildeId = kilde.KildeId,
        Navn = kilde.Navn,
        Type = kilde.Type,
        Autoritativ = kilde.Autoritativ,
        RettskildeIder = kilde.KildeRettskilde.Select(kr => kr.RettskildeId).ToList(),
        CccevReferanse = kilde.CccevReferanse,
        ErLaast = await _repository.ErKildeReferertAsync(kilde.KildeId, ct)
    };
}
