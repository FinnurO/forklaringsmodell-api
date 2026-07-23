using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class RegelService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettRegelDto> _validator;

    public RegelService(IForklaringsmodellRepository repository, IValidator<OpprettRegelDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<RegelDto>> ListAsync(CancellationToken ct = default)
    {
        var regler = await _repository.GetReglerAsync(ct);
        var dtos = new List<RegelDto>();
        foreach (var regel in regler)
        {
            dtos.Add(await ToDtoAsync(regel, ct));
        }
        return dtos;
    }

    public async Task<RegelDto> OpprettAsync(OpprettRegelDto dto, CancellationToken ct = default)
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

        var regel = new Regel
        {
            RegelId = Guid.NewGuid(),
            Teknologi = dto.Teknologi,
            Type = dto.Type,
            CpsvRegelReferanse = dto.CpsvRegelReferanse,
            RegeldefinisjonReferanse = dto.RegeldefinisjonReferanse
        };

        foreach (var rettskilde in rettskilder)
        {
            regel.RegelRettskilde.Add(new RegelRettskilde { RegelId = regel.RegelId, RettskildeId = rettskilde.RettskildeId });
        }

        await _repository.AddRegelAsync(regel, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(regel, ct);
    }

    private async Task<RegelDto> ToDtoAsync(Regel regel, CancellationToken ct) => new()
    {
        RegelId = regel.RegelId,
        RettskildeIder = regel.RegelRettskilde.Select(rr => rr.RettskildeId).ToList(),
        Teknologi = regel.Teknologi,
        Type = regel.Type,
        CpsvRegelReferanse = regel.CpsvRegelReferanse,
        RegeldefinisjonReferanse = regel.RegeldefinisjonReferanse,
        ErLaast = await _repository.ErRegelReferertAsync(regel.RegelId, ct)
    };
}
