using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Mapping;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;

namespace Forklaringsmodell.Application.Services;

/// <summary>
/// Setter sammen forklaringen i ett svar, slik at en visning slipper å gjøre en rekke oppslag.
/// Det finnes to visninger med ulik garanti: vedtak-forklaringen er det frosne øyeblikksbildet
/// (kun det forklaringsloggen peker på), sak-forklaringen er en levende visning av hele saken
/// der <c>ErLaast</c> viser hva som er frosset. Begge deler samme utvidelse: oppløst
/// referansedata, kryss-sak-referanser og vurderingstreet.
/// </summary>
public class ForklaringService
{
    private readonly IForklaringsmodellRepository _repository;

    public ForklaringService(IForklaringsmodellRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Leser hydrert forklaring for ett vedtak (GET /api/vedtak/{id}/forklaring).</summary>
    public async Task<HydrertForklaringDto> GetVedtakForklaringAsync(Guid vedtakId, CancellationToken ct = default)
    {
        var vedtak = await _repository.GetVedtakAsync(vedtakId, ct) ?? throw new NotFoundException($"Vedtak {vedtakId} finnes ikke.");
        var logg = await _repository.GetForklaringsloggAsync(vedtakId, ct) ?? throw new NotFoundException($"Forklaringslogg for vedtak {vedtakId} finnes ikke.");

        var faktumIder = IderAvType(logg, OppforingsType.Faktum);
        var vurderingIder = IderAvType(logg, OppforingsType.Vurdering);
        var partsmedvirkningIder = IderAvType(logg, OppforingsType.Partsmedvirkning);

        var faktum = faktumIder.Count > 0 ? await _repository.GetFaktumByIderAsync(faktumIder, ct) : new List<Faktum>();
        var vurderinger = vurderingIder.Count > 0 ? await _repository.GetVurderingerByIderAsync(vurderingIder, ct) : new List<Vurdering>();
        var partsmedvirkninger = partsmedvirkningIder.Count > 0 ? await _repository.GetPartsmedvirkningerByIderAsync(partsmedvirkningIder, ct) : new List<Partsmedvirkning>();
        var virkninger = await _repository.GetVirkningerForVedtakAsync(vedtakId, ct);

        var utvidelse = await UtvidAsync(faktum, vurderinger, virkninger, ct);

        return new HydrertForklaringDto
        {
            Vedtak = DtoMapper.TilDto(vedtak),
            Forklaringslogg = DtoMapper.TilDto(logg),
            Faktum = faktum.Select(f => DtoMapper.TilDto(f, erLaast: true)).ToList(),
            Vurderinger = vurderinger.Select(v => DtoMapper.TilDto(v, erLaast: true)).ToList(),
            Vurderingstre = DtoMapper.ByggTre(vurderinger, _ => true),
            Partsmedvirkninger = partsmedvirkninger.Select(DtoMapper.TilDto).ToList(),
            Virkninger = virkninger.Select(DtoMapper.TilDto).ToList(),
            RefererteVurderinger = utvidelse.RefererteVurderinger,
            AndreFaktum = utvidelse.AndreFaktum,
            Referansedata = utvidelse.Referansedata
        };
    }

    /// <summary>Leser hele saken samlet (GET /api/saker/{sakId}/forklaring), også uten vedtak.</summary>
    public async Task<SakForklaringDto> GetSakForklaringAsync(Guid sakId, CancellationToken ct = default)
    {
        var sak = await _repository.GetSakAsync(sakId, ct) ?? throw new NotFoundException($"Sak {sakId} finnes ikke.");

        var relasjoner = await _repository.GetSakRelasjonerForSakAsync(sakId, ct);
        var faktum = await _repository.GetFaktumForSakAsync(sakId, ct);
        var vurderinger = await _repository.GetVurderingerForSakAsync(sakId, ct);
        var partsmedvirkninger = await _repository.GetPartsmedvirkningerForSakAsync(sakId, ct);
        var vedtakListe = await _repository.GetVedtakForSakAsync(sakId, ct);

        var vedtakMedVirkninger = new List<VedtakMedVirkningerDto>();
        var alleVirkninger = new List<Vedtaksvirkning>();
        var frosneFaktum = new HashSet<Guid>();
        var frosneVurderinger = new HashSet<Guid>();

        foreach (var vedtak in vedtakListe)
        {
            var logg = await _repository.GetForklaringsloggAsync(vedtak.VedtakId, ct);
            var virkninger = await _repository.GetVirkningerForVedtakAsync(vedtak.VedtakId, ct);
            alleVirkninger.AddRange(virkninger);

            if (logg is not null)
            {
                frosneFaktum.UnionWith(IderAvType(logg, OppforingsType.Faktum));
                frosneVurderinger.UnionWith(IderAvType(logg, OppforingsType.Vurdering));
            }

            vedtakMedVirkninger.Add(new VedtakMedVirkningerDto
            {
                Vedtak = DtoMapper.TilDto(vedtak),
                Forklaringslogg = logg is null ? new ForklaringsloggDto { VedtakId = vedtak.VedtakId } : DtoMapper.TilDto(logg),
                Virkninger = virkninger.Select(DtoMapper.TilDto).ToList()
            });
        }

        var utvidelse = await UtvidAsync(faktum, vurderinger, alleVirkninger, ct);

        return new SakForklaringDto
        {
            Sak = DtoMapper.TilDto(sak),
            Oppsummering = new SakOppsummeringDto
            {
                HarVedtak = vedtakListe.Count > 0,
                AntallVedtak = vedtakListe.Count,
                AntallFaktum = faktum.Count,
                AntallVurderinger = vurderinger.Count,
                AntallEskalerteVurderinger = vurderinger.Count(v => v.Eskalert)
            },
            Relasjoner = relasjoner.Select(DtoMapper.TilDto).ToList(),
            Faktum = faktum.Select(f => DtoMapper.TilDto(f, frosneFaktum.Contains(f.FaktumId))).ToList(),
            Vurderinger = vurderinger.Select(v => DtoMapper.TilDto(v, frosneVurderinger.Contains(v.VurderingId))).ToList(),
            Vurderingstre = DtoMapper.ByggTre(vurderinger, v => frosneVurderinger.Contains(v.VurderingId)),
            Partsmedvirkninger = partsmedvirkninger.Select(DtoMapper.TilDto).ToList(),
            Vedtak = vedtakMedVirkninger,
            RefererteVurderinger = utvidelse.RefererteVurderinger,
            AndreFaktum = utvidelse.AndreFaktum,
            Referansedata = utvidelse.Referansedata
        };
    }

    private static List<Guid> IderAvType(Forklaringslogg logg, OppforingsType type) =>
        logg.Oppforinger.Where(o => o.Type == type).Select(o => o.ReferanseId).Distinct().ToList();

    private sealed record Utvidelse(
        List<VurderingDto> RefererteVurderinger,
        List<FaktumDto> AndreFaktum,
        ReferansedataDto Referansedata);

    /// <summary>
    /// Finner det forklaringen peker på utenfor de egne radene, og løser opp referansedata:
    /// vurderinger i andre saker (ett nivå, regel 3.11), faktum utenfor egen liste, og så
    /// kilder, regler, vilkår og rettskilder (inkludert rettskildene disse selv peker på).
    /// </summary>
    private async Task<Utvidelse> UtvidAsync(
        IReadOnlyCollection<Faktum> faktum,
        IReadOnlyCollection<Vurdering> vurderinger,
        IReadOnlyCollection<Vedtaksvirkning> virkninger,
        CancellationToken ct)
    {
        var egneVurderingIder = vurderinger.Select(v => v.VurderingId).ToHashSet();
        var refererteIder = vurderinger
            .SelectMany(v => v.RefererteVurderinger.Select(r => r.RefererteVurderingId))
            .Where(id => !egneVurderingIder.Contains(id))
            .Distinct()
            .ToList();
        var refererte = refererteIder.Count > 0
            ? await _repository.GetVurderingerByIderAsync(refererteIder, ct)
            : new List<Vurdering>();
        var alleVurderinger = vurderinger.Concat(refererte).ToList();

        var egneFaktumIder = faktum.Select(f => f.FaktumId).ToHashSet();
        var andreFaktumIder = alleVurderinger.SelectMany(v => v.VurderingFaktum.Select(vf => vf.FaktumId))
            .Concat(virkninger.SelectMany(v => v.VedtaksvirkningFaktum.Select(vf => vf.FaktumId)))
            .Where(id => !egneFaktumIder.Contains(id))
            .Distinct()
            .ToList();
        var andreFaktum = andreFaktumIder.Count > 0
            ? await _repository.GetFaktumByIderAsync(andreFaktumIder, ct)
            : new List<Faktum>();
        var alleFaktum = faktum.Concat(andreFaktum).ToList();

        var vilkarIder = alleVurderinger.Where(v => v.VilkarId.HasValue).Select(v => v.VilkarId!.Value)
            .Concat(virkninger.Where(v => v.VilkarId.HasValue).Select(v => v.VilkarId!.Value))
            .Distinct().ToList();
        var vilkar = vilkarIder.Count > 0 ? await _repository.GetVilkarByIderAsync(vilkarIder, ct) : new List<Vilkar>();

        var regelIder = alleVurderinger.Select(v => v.RegelId)
            .Concat(vilkar.Where(v => v.RegelId.HasValue).Select(v => v.RegelId!.Value))
            .Distinct().ToList();
        var regler = regelIder.Count > 0 ? await _repository.GetReglerByIderAsync(regelIder, ct) : new List<Regel>();

        var kildeIder = alleFaktum.Select(f => f.KildeId).Distinct().ToList();
        var kilder = kildeIder.Count > 0 ? await _repository.GetKilderByIderAsync(kildeIder, ct) : new List<Kilde>();

        var rettskildeIder = alleFaktum.SelectMany(f => f.FaktumRettskilde.Select(r => r.RettskildeId))
            .Concat(alleVurderinger.SelectMany(v => v.VurderingRettskilde.Select(r => r.RettskildeId)))
            .Concat(kilder.SelectMany(k => k.KildeRettskilde.Select(r => r.RettskildeId)))
            .Concat(regler.SelectMany(r => r.RegelRettskilde.Select(rr => rr.RettskildeId)))
            .Concat(vilkar.SelectMany(v => v.VilkarRettskilde.Select(vr => vr.RettskildeId)))
            .Distinct().ToList();
        var rettskilder = rettskildeIder.Count > 0 ? await _repository.GetRettskilderByIderAsync(rettskildeIder, ct) : new List<Rettskilde>();

        var refererteDtos = new List<VurderingDto>();
        foreach (var v in refererte)
        {
            refererteDtos.Add(DtoMapper.TilDto(v, await _repository.ErVurderingReferertAsync(v.VurderingId, ct)));
        }

        var andreFaktumDtos = new List<FaktumDto>();
        foreach (var f in andreFaktum)
        {
            andreFaktumDtos.Add(DtoMapper.TilDto(f, await _repository.ErFaktumReferertAsync(f.FaktumId, ct)));
        }

        var referansedata = new ReferansedataDto();
        foreach (var k in kilder.OrderBy(k => k.Navn))
        {
            referansedata.Kilder.Add(DtoMapper.TilDto(k, await _repository.ErKildeReferertAsync(k.KildeId, ct)));
        }
        foreach (var r in regler.OrderBy(r => r.Teknologi))
        {
            referansedata.Regler.Add(DtoMapper.TilDto(r, await _repository.ErRegelReferertAsync(r.RegelId, ct)));
        }
        foreach (var v in vilkar.OrderBy(v => v.Navn))
        {
            referansedata.Vilkar.Add(DtoMapper.TilDto(v, await _repository.ErVilkarReferertAsync(v.VilkarId, ct)));
        }
        referansedata.Rettskilder.AddRange(rettskilder.OrderBy(r => r.Henvisning).Select(DtoMapper.TilDto));

        return new Utvidelse(refererteDtos, andreFaktumDtos, referansedata);
    }
}
