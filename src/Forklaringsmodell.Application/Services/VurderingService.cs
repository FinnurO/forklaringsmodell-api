using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Options;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Forklaringsmodell.Application.Services;

public class VurderingService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettVurderingDto> _validator;
    private readonly KonfidensTerskelOptions _terskelOptions;

    public VurderingService(
        IForklaringsmodellRepository repository,
        IValidator<OpprettVurderingDto> validator,
        IOptions<KonfidensTerskelOptions> terskelOptions)
    {
        _repository = repository;
        _validator = validator;
        _terskelOptions = terskelOptions.Value;
    }

    public async Task<List<VurderingDto>> ListForSakAsync(Guid sakId, CancellationToken ct = default)
    {
        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        var vurderinger = await _repository.GetVurderingerForSakAsync(sak.SakId, ct);
        var dtos = new List<VurderingDto>();
        foreach (var v in vurderinger)
        {
            dtos.Add(await ToDtoAsync(v, ct));
        }
        return dtos;
    }

    public async Task<VurderingDto> GetAsync(Guid vurderingId, CancellationToken ct = default)
    {
        var vurdering = await _repository.GetVurderingAsync(vurderingId, ct) ?? throw new NotFoundException($"Vurdering {vurderingId} finnes ikke.");
        return await ToDtoAsync(vurdering, ct);
    }

    public async Task<VurderingDto> OpprettAsync(Guid sakId, OpprettVurderingDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");
        var regel = await _repository.GetRegelAsync(dto.RegelId, ct) ?? throw new NotFoundException($"Regel {dto.RegelId} finnes ikke.");

        var faktumRader = dto.FaktumIder.Count > 0
            ? await _repository.GetFaktumByIderAsync(dto.FaktumIder, ct)
            : new List<Faktum>();

        var manglende = dto.FaktumIder.Except(faktumRader.Select(f => f.FaktumId)).ToList();
        if (manglende.Count > 0)
        {
            throw new NotFoundException($"Faktum {string.Join(", ", manglende)} finnes ikke.");
        }

        // Regel 3.11: kryss-sak-referanser (Faktum fra en annen Sak) skal kun peke til
        // rader som allerede er del av en frosset Forklaringslogg i den relaterte saken.
        // Faktum i samme sak har ingen slik restriksjon (vanlig, ufrosset saksbehandling).
        foreach (var faktum in faktumRader.Where(f => f.SakId != sakId))
        {
            if (!await _repository.ErFaktumReferertAsync(faktum.FaktumId, ct))
            {
                throw new AppendOnlyViolationException(
                    $"Faktum {faktum.FaktumId} tilhører en annen sak ({faktum.SakId}) og er ikke del av en frosset Forklaringslogg der ennå — kan ikke refereres.");
            }
        }

        var refererteVurderinger = dto.RefererteVurderingIder.Count > 0
            ? await _repository.GetVurderingerByIderAsync(dto.RefererteVurderingIder, ct)
            : new List<Vurdering>();
        var manglendeReferanser = dto.RefererteVurderingIder.Except(refererteVurderinger.Select(v => v.VurderingId)).ToList();
        if (manglendeReferanser.Count > 0)
        {
            throw new NotFoundException($"Vurdering {string.Join(", ", manglendeReferanser)} finnes ikke.");
        }

        // Regel 3.11: RefererteVurderingIder skal kun peke til vurderinger som allerede
        // er del av en frosset Forklaringslogg (typisk i en annen, allerede avsluttet sak).
        foreach (var referertVurdering in refererteVurderinger)
        {
            if (!await _repository.ErVurderingReferertAsync(referertVurdering.VurderingId, ct))
            {
                throw new AppendOnlyViolationException(
                    $"Vurdering {referertVurdering.VurderingId} er ikke del av en frosset Forklaringslogg ennå — kan ikke refereres via RefererteVurderingIder.");
            }
        }

        var rettskilder = dto.RettskildeIder.Count > 0
            ? await _repository.GetRettskilderByIderAsync(dto.RettskildeIder, ct)
            : new List<Rettskilde>();
        var manglendeRettskilder = dto.RettskildeIder.Except(rettskilder.Select(r => r.RettskildeId)).ToList();
        if (manglendeRettskilder.Count > 0)
        {
            throw new NotFoundException($"Rettskilde {string.Join(", ", manglendeRettskilder)} finnes ikke.");
        }

        var eskalert = dto.Eskalert;

        // Pragmatisk tolkning av regel 3.3: terskelen for eskalering leses fra
        // konfigurasjon (KonfidensTerskelOptions), nøkkelbasert på Regel.RegelId med en
        // global default. Hvis en GenerativKI-vurdering har konfidens under terskelen,
        // markeres den automatisk som eskalert (i tillegg til det klienten selv sender
        // inn) — konsistent med at Vurdering.Type "kan avvike fra Regel.Type ved
        // eskalering" i domenemodellens kommentar. Dette signalet brukes videre av
        // VedtakService (regel 3.5) når AutomatiseringsGrad beregnes.
        if (dto.Type == VurderingsType.GenerativKI && dto.Konfidens.HasValue)
        {
            var terskel = _terskelOptions.ForRegel(dto.RegelId);
            if (dto.Konfidens.Value < terskel)
            {
                eskalert = true;
            }
        }

        var vurdering = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regel.RegelId,
            Type = dto.Type,
            Beregningsspor = dto.Beregningsspor,
            Konfidens = dto.Konfidens,
            Eskalert = eskalert,
            Hovedhensyn = dto.Hovedhensyn,
            ForkastedeUtfall = dto.ForkastedeUtfall
        };

        foreach (var faktum in faktumRader)
        {
            vurdering.VurderingFaktum.Add(new VurderingFaktum { VurderingId = vurdering.VurderingId, FaktumId = faktum.FaktumId });
        }

        foreach (var rettskilde in rettskilder)
        {
            vurdering.VurderingRettskilde.Add(new VurderingRettskilde { VurderingId = vurdering.VurderingId, RettskildeId = rettskilde.RettskildeId });
        }

        foreach (var referertVurdering in refererteVurderinger)
        {
            vurdering.RefererteVurderinger.Add(new VurderingReferanse { VurderingId = vurdering.VurderingId, RefererteVurderingId = referertVurdering.VurderingId });
        }

        await _repository.AddVurderingAsync(vurdering, ct);
        await _repository.SaveChangesAsync(ct);
        return await ToDtoAsync(vurdering, ct);
    }

    /// <summary>
    /// Regel 3.1: dersom en Vurdering allerede er referert av en ForklaringsloggOppforing,
    /// skal endring avvises. Se tilsvarende kommentar i FaktumService.
    /// </summary>
    public async Task SjekkKanEndreAsync(Guid vurderingId, CancellationToken ct = default)
    {
        var erReferert = await _repository.ErVurderingReferertAsync(vurderingId, ct);
        if (erReferert)
        {
            throw new AppendOnlyViolationException(
                $"Vurdering {vurderingId} er referert av en ForklaringsloggOppforing og kan ikke endres.");
        }
    }

    private async Task<VurderingDto> ToDtoAsync(Vurdering vurdering, CancellationToken ct) => new()
    {
        VurderingId = vurdering.VurderingId,
        SakId = vurdering.SakId,
        RegelId = vurdering.RegelId,
        Type = vurdering.Type,
        Beregningsspor = vurdering.Beregningsspor,
        Konfidens = vurdering.Konfidens,
        Eskalert = vurdering.Eskalert,
        Hovedhensyn = vurdering.Hovedhensyn,
        ForkastedeUtfall = vurdering.ForkastedeUtfall,
        ErLaast = await _repository.ErVurderingReferertAsync(vurdering.VurderingId, ct),
        FaktumIder = vurdering.VurderingFaktum.Select(vf => vf.FaktumId).ToList(),
        RettskildeIder = vurdering.VurderingRettskilde.Select(vr => vr.RettskildeId).ToList(),
        RefererteVurderingIder = vurdering.RefererteVurderinger.Select(r => r.RefererteVurderingId).ToList()
    };
}
