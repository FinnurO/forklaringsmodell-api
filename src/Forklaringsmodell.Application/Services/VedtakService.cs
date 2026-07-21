using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using FluentValidation;

namespace Forklaringsmodell.Application.Services;

public class VedtakService
{
    private readonly IForklaringsmodellRepository _repository;
    private readonly IValidator<OpprettVedtakDto> _validator;

    public VedtakService(IForklaringsmodellRepository repository, IValidator<OpprettVedtakDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    /// <summary>
    /// Oppretter Vedtak + Forklaringslogg + ForklaringsloggOppforing-rader i én
    /// transaksjon (regel 3.5/3.6-relevant flyt, se spesifikasjonens punkt 5). Alt som
    /// refereres her blir fra dette punktet append-only (regel 3.1): efterfølgende forsøk
    /// på å endre de refererte Faktum-/Vurdering-/Regel-radene skal kastes tilbake som
    /// AppendOnlyViolationException fra de respektive servicene.
    /// </summary>
    public async Task<VedtakDto> OpprettAsync(Guid sakId, OpprettVedtakDto dto, CancellationToken ct = default)
    {
        var result = await _validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");

        var faktumRader = dto.FaktumIder.Count > 0
            ? await _repository.GetFaktumByIderAsync(dto.FaktumIder, ct)
            : new List<Faktum>();
        var manglendeFaktum = dto.FaktumIder.Except(faktumRader.Select(f => f.FaktumId)).ToList();
        if (manglendeFaktum.Count > 0)
        {
            throw new NotFoundException($"Faktum {string.Join(", ", manglendeFaktum)} finnes ikke.");
        }

        var vurderingRader = dto.VurderingIder.Count > 0
            ? await _repository.GetVurderingerByIderAsync(dto.VurderingIder, ct)
            : new List<Vurdering>();
        var manglendeVurdering = dto.VurderingIder.Except(vurderingRader.Select(v => v.VurderingId)).ToList();
        if (manglendeVurdering.Count > 0)
        {
            throw new NotFoundException($"Vurdering {string.Join(", ", manglendeVurdering)} finnes ikke.");
        }

        var partsmedvirkningRader = dto.PartsmedvirkningIder.Count > 0
            ? await _repository.GetPartsmedvirkningerByIderAsync(dto.PartsmedvirkningIder, ct)
            : new List<Partsmedvirkning>();
        var manglendePartsmedvirkning = dto.PartsmedvirkningIder.Except(partsmedvirkningRader.Select(p => p.MedvirkningId)).ToList();
        if (manglendePartsmedvirkning.Count > 0)
        {
            throw new NotFoundException($"Partsmedvirkning {string.Join(", ", manglendePartsmedvirkning)} finnes ikke.");
        }

        var transaction = await _repository.BeginTransactionAsync(ct);

        var vedtak = new Vedtak
        {
            VedtakId = Guid.NewGuid(),
            SakId = sak.SakId,
            Tidspunkt = DateTimeOffset.UtcNow,
            Utfall = dto.Utfall,
            AutomatiseringsGrad = BeregnAutomatiseringsGrad(vurderingRader)
        };

        var logg = new Forklaringslogg
        {
            LoggId = Guid.NewGuid(),
            VedtakId = vedtak.VedtakId
        };

        foreach (var faktum in faktumRader)
        {
            logg.Oppforinger.Add(new ForklaringsloggOppforing
            {
                OppforingId = Guid.NewGuid(),
                LoggId = logg.LoggId,
                Type = OppforingsType.Faktum,
                ReferanseId = faktum.FaktumId
            });
        }

        foreach (var vurdering in vurderingRader)
        {
            logg.Oppforinger.Add(new ForklaringsloggOppforing
            {
                OppforingId = Guid.NewGuid(),
                LoggId = logg.LoggId,
                Type = OppforingsType.Vurdering,
                ReferanseId = vurdering.VurderingId
            });
        }

        foreach (var partsmedvirkning in partsmedvirkningRader)
        {
            logg.Oppforinger.Add(new ForklaringsloggOppforing
            {
                OppforingId = Guid.NewGuid(),
                LoggId = logg.LoggId,
                Type = OppforingsType.Partsmedvirkning,
                ReferanseId = partsmedvirkning.MedvirkningId
            });
        }

        await _repository.AddVedtakMedForklaringsloggAsync(vedtak, logg, ct);
        await _repository.SaveChangesAsync(ct);

        // Transaksjonen committer på disposal (se ForklaringsmodellRepository.TransactionWrapper) -
        // dette må skje etter SaveChangesAsync ovenfor slik at Vedtak/Forklaringslogg/
        // ForklaringsloggOppforing fryses atomisk i samme databasetransaksjon.
        await transaction.DisposeAsync();

        return ToDto(vedtak);
    }

    public async Task<VedtakDto> GetAsync(Guid vedtakId, CancellationToken ct = default)
    {
        var vedtak = await _repository.GetVedtakAsync(vedtakId, ct) ?? throw new NotFoundException($"Vedtak {vedtakId} finnes ikke.");
        return ToDto(vedtak);
    }

    /// <summary>Leser hydrert forklaring: vedtak + alle refererte faktum/vurdering/partsmedvirkning-rader utfoldet.</summary>
    public async Task<HydrertForklaringDto> GetForklaringAsync(Guid vedtakId, CancellationToken ct = default)
    {
        var vedtak = await _repository.GetVedtakAsync(vedtakId, ct) ?? throw new NotFoundException($"Vedtak {vedtakId} finnes ikke.");
        var logg = await _repository.GetForklaringsloggAsync(vedtakId, ct) ?? throw new NotFoundException($"Forklaringslogg for vedtak {vedtakId} finnes ikke.");

        var faktumIder = logg.Oppforinger.Where(o => o.Type == OppforingsType.Faktum).Select(o => o.ReferanseId).ToList();
        var vurderingIder = logg.Oppforinger.Where(o => o.Type == OppforingsType.Vurdering).Select(o => o.ReferanseId).ToList();
        var partsmedvirkningIder = logg.Oppforinger.Where(o => o.Type == OppforingsType.Partsmedvirkning).Select(o => o.ReferanseId).ToList();

        var faktumRader = faktumIder.Count > 0 ? await _repository.GetFaktumByIderAsync(faktumIder, ct) : new List<Faktum>();
        var vurderingRader = vurderingIder.Count > 0 ? await _repository.GetVurderingerByIderAsync(vurderingIder, ct) : new List<Vurdering>();
        var partsmedvirkningRader = partsmedvirkningIder.Count > 0 ? await _repository.GetPartsmedvirkningerByIderAsync(partsmedvirkningIder, ct) : new List<Partsmedvirkning>();

        return new HydrertForklaringDto
        {
            Vedtak = ToDto(vedtak),
            Forklaringslogg = new ForklaringsloggDto
            {
                LoggId = logg.LoggId,
                VedtakId = logg.VedtakId,
                Oppforinger = logg.Oppforinger.Select(o => new ForklaringsloggOppforingDto
                {
                    OppforingId = o.OppforingId,
                    Type = o.Type,
                    ReferanseId = o.ReferanseId
                }).ToList()
            },
            Faktum = faktumRader.Select(f => new FaktumDto
            {
                FaktumId = f.FaktumId,
                SakId = f.SakId,
                KildeId = f.KildeId,
                Type = f.Type,
                Struktur = f.Struktur,
                Verdi = f.Verdi,
                AvledetFraFaktumId = f.AvledetFraFaktumId,
                InnhentetTidspunkt = f.InnhentetTidspunkt,
                ErLaast = true
            }).ToList(),
            Vurderinger = vurderingRader.Select(v => new VurderingDto
            {
                VurderingId = v.VurderingId,
                SakId = v.SakId,
                RegelId = v.RegelId,
                Type = v.Type,
                Beregningsspor = v.Beregningsspor,
                Konfidens = v.Konfidens,
                Eskalert = v.Eskalert,
                Hovedhensyn = v.Hovedhensyn,
                ForkastedeUtfall = v.ForkastedeUtfall,
                ErLaast = true,
                FaktumIder = v.VurderingFaktum.Select(vf => vf.FaktumId).ToList()
            }).ToList(),
            Partsmedvirkninger = partsmedvirkningRader.Select(p => new PartsmedvirkningDto
            {
                MedvirkningId = p.MedvirkningId,
                SakId = p.SakId,
                Type = p.Type,
                Tidspunkt = p.Tidspunkt,
                Innhold = p.Innhold
            }).ToList()
        };
    }

    /// <summary>
    /// Regel 3.5: Vedtak.AutomatiseringsGrad skal ikke settes fritt av klienten, men
    /// beregnes serverside ut fra andelen Vurdering som bærer et manuelt/eskalert signal
    /// blant de refererte vurderingene. Vi tolker "andelen Vurdering med Type == Skjonn og
    /// Eskalert == true" som to signaler som begge skal telle med (OR, ikke AND) - en
    /// Vurdering telles som "ikke helautomatisert" hvis den enten er Type == Skjonn
    /// (menneskelig skjønn er alltid manuelt) ELLER har Eskalert == true (en opprinnelig
    /// automatisert/GenerativKI-vurdering som ble eskalert til manuell behandling, se
    /// VurderingService for hvordan lav konfidens automatisk setter Eskalert). Dette
    /// stemmer med eksempeldataen i spesifikasjonens punkt 6, der en GenerativKI-vurdering
    /// med Eskalert == true (og som altså IKKE er Type == Skjonn) bidrar til
    /// DelvisAutomatisert sammen med en ekte Skjonn-vurdering.
    ///   - Ingen vurderinger => Manuell (konservativt default, ingen automatisert
    ///     grunnlag er dokumentert).
    ///   - Andel skjønn/eskalert == 0 => Helautomatisert.
    ///   - Andel skjønn/eskalert == 1 (alle) => Manuell.
    ///   - Ellers => DelvisAutomatisert.
    /// </summary>
    private static AutomatiseringsGrad BeregnAutomatiseringsGrad(IReadOnlyCollection<Vurdering> vurderinger)
    {
        if (vurderinger.Count == 0)
        {
            return AutomatiseringsGrad.Manuell;
        }

        var manuelleEllerEskalerte = vurderinger.Count(v => v.Type == VurderingsType.Skjonn || v.Eskalert);
        if (manuelleEllerEskalerte == 0)
        {
            return AutomatiseringsGrad.Helautomatisert;
        }

        if (manuelleEllerEskalerte == vurderinger.Count)
        {
            return AutomatiseringsGrad.Manuell;
        }

        return AutomatiseringsGrad.DelvisAutomatisert;
    }

    private static VedtakDto ToDto(Vedtak vedtak) => new()
    {
        VedtakId = vedtak.VedtakId,
        SakId = vedtak.SakId,
        Tidspunkt = vedtak.Tidspunkt,
        Utfall = vedtak.Utfall,
        AutomatiseringsGrad = vedtak.AutomatiseringsGrad
    };
}
