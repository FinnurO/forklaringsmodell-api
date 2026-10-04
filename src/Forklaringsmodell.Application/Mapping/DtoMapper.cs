using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Domain.Entities;

namespace Forklaringsmodell.Application.Mapping;

/// <summary>
/// Én felles plass for entitet-til-DTO-mapping. Alle services og forklaringsvisningene går
/// via denne, slik at et nytt DTO-felt kun må kobles inn ett sted (tidligere lå
/// projeksjonene duplisert og falt ut av takt med DTO-ene). <c>erLaast</c> er beregnet av
/// kalleren, siden det avhenger av forklaringsloggene og ikke av raden selv.
/// </summary>
public static class DtoMapper
{
    public static SakDto TilDto(Sak sak) => new()
    {
        SakId = sak.SakId,
        Tittel = sak.Tittel,
        Status = sak.Status,
        Opprettet = sak.Opprettet,
        SistEndret = sak.SistEndret,
        CpsvTjenesteReferanse = sak.CpsvTjenesteReferanse,
        UtlosendeHendelse = sak.UtlosendeHendelse
    };

    public static SakRelasjonDto TilDto(SakRelasjon relasjon) => new()
    {
        RelasjonId = relasjon.RelasjonId,
        SakId = relasjon.SakId,
        RelatertSakId = relasjon.RelatertSakId,
        Type = relasjon.Type
    };

    public static RettskildeDto TilDto(Rettskilde rettskilde) => new()
    {
        RettskildeId = rettskilde.RettskildeId,
        Type = rettskilde.Type,
        Henvisning = rettskilde.Henvisning,
        VersjonDato = rettskilde.VersjonDato,
        EliReferanse = rettskilde.EliReferanse
    };

    public static KildeDto TilDto(Kilde kilde, bool erLaast) => new()
    {
        KildeId = kilde.KildeId,
        Navn = kilde.Navn,
        Type = kilde.Type,
        Autoritativ = kilde.Autoritativ,
        RettskildeIder = kilde.KildeRettskilde.Select(kr => kr.RettskildeId).ToList(),
        CccevReferanse = kilde.CccevReferanse,
        ErLaast = erLaast
    };

    public static RegelDto TilDto(Regel regel, bool erLaast) => new()
    {
        RegelId = regel.RegelId,
        RettskildeIder = regel.RegelRettskilde.Select(rr => rr.RettskildeId).ToList(),
        Teknologi = regel.Teknologi,
        Type = regel.Type,
        CpsvRegelReferanse = regel.CpsvRegelReferanse,
        RegeldefinisjonReferanse = regel.RegeldefinisjonReferanse,
        ErLaast = erLaast
    };

    public static VilkarDto TilDto(Vilkar vilkar, bool erLaast) => new()
    {
        VilkarId = vilkar.VilkarId,
        Navn = vilkar.Navn,
        Kode = vilkar.Kode,
        Kodeverk = vilkar.Kodeverk,
        Type = vilkar.Type,
        Grunnlagstype = vilkar.Grunnlagstype,
        Fastsettelsesmate = vilkar.Fastsettelsesmate,
        StandardTekst = FlerspraakligTekstMapper.TilDto(vilkar.StandardTekst),
        RettskildeIder = vilkar.VilkarRettskilde.Select(vr => vr.RettskildeId).ToList(),
        RegelId = vilkar.RegelId,
        CpsvTjenesteReferanse = vilkar.CpsvTjenesteReferanse,
        ErLaast = erLaast
    };

    public static FaktumDto TilDto(Faktum faktum, bool erLaast) => new()
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
        ErLaast = erLaast
    };

    public static VurderingDto TilDto(Vurdering vurdering, bool erLaast)
    {
        var dto = new VurderingDto();
        Fyll(dto, vurdering, erLaast);
        return dto;
    }

    /// <summary>Samme felter som <see cref="TilDto(Vurdering, bool)"/>, men uten delvurderingene utfoldet (se <see cref="ByggTre"/>).</summary>
    public static VurderingNodeDto TilNode(Vurdering vurdering, bool erLaast)
    {
        var node = new VurderingNodeDto();
        Fyll(node, vurdering, erLaast);
        return node;
    }

    private static void Fyll(VurderingDto dto, Vurdering v, bool erLaast)
    {
        dto.VurderingId = v.VurderingId;
        dto.SakId = v.SakId;
        dto.RegelId = v.RegelId;
        dto.Type = v.Type;
        dto.Utfall = v.Utfall;
        dto.Beregningsspor = v.Beregningsspor;
        dto.Konfidens = v.Konfidens;
        dto.Eskalert = v.Eskalert;
        dto.Hovedhensyn = FlerspraakligTekstMapper.TilDto(v.HovedhensynTekst);
        dto.ForkastedeUtfall = FlerspraakligTekstMapper.TilDto(v.ForkastedeUtfallTekst);
        dto.VilkarId = v.VilkarId;
        dto.ForelderVurderingId = v.ForelderVurderingId;
        dto.ErLaast = erLaast;
        dto.FaktumIder = v.VurderingFaktum.Select(vf => vf.FaktumId).ToList();
        dto.RettskildeIder = v.VurderingRettskilde.Select(vr => vr.RettskildeId).ToList();
        dto.RefererteVurderingIder = v.RefererteVurderinger.Select(r => r.RefererteVurderingId).ToList();
        dto.DelvurderingIder = v.Delvurderinger.Select(d => d.VurderingId).ToList();
    }

    /// <summary>
    /// Nøster en flat vurderingsliste til et tre via <c>ForelderVurderingId</c> (regel 3.18).
    /// En vurdering blir rot når den ikke har noen forelder, eller når foreldren ikke er med i
    /// listen (for eksempel en delmengde valgt til et vedtak). Rekkefølgen følger listen.
    /// </summary>
    public static List<VurderingNodeDto> ByggTre(IReadOnlyCollection<Vurdering> vurderinger, Func<Vurdering, bool> erLaast)
    {
        var noder = vurderinger.ToDictionary(v => v.VurderingId, v => TilNode(v, erLaast(v)));
        var røtter = new List<VurderingNodeDto>();

        foreach (var v in vurderinger)
        {
            var node = noder[v.VurderingId];
            if (v.ForelderVurderingId is { } forelderId && noder.TryGetValue(forelderId, out var forelder))
            {
                forelder.Delvurderinger.Add(node);
            }
            else
            {
                røtter.Add(node);
            }
        }

        return røtter;
    }

    public static PartsmedvirkningDto TilDto(Partsmedvirkning p) => new()
    {
        MedvirkningId = p.MedvirkningId,
        SakId = p.SakId,
        Type = p.Type,
        Tidspunkt = p.Tidspunkt,
        Innhold = p.Innhold
    };

    public static VedtakDto TilDto(Vedtak vedtak) => new()
    {
        VedtakId = vedtak.VedtakId,
        SakId = vedtak.SakId,
        Tidspunkt = vedtak.Tidspunkt,
        Utfall = vedtak.Utfall,
        AutomatiseringsGrad = vedtak.AutomatiseringsGrad
    };

    public static ForklaringsloggDto TilDto(Forklaringslogg logg) => new()
    {
        LoggId = logg.LoggId,
        VedtakId = logg.VedtakId,
        Oppforinger = logg.Oppforinger.Select(o => new ForklaringsloggOppforingDto
        {
            OppforingId = o.OppforingId,
            Type = o.Type,
            ReferanseId = o.ReferanseId
        }).ToList()
    };

    public static VedtaksvirkningDto TilDto(Vedtaksvirkning virkning) => new()
    {
        VirkningId = virkning.VirkningId,
        VedtakId = virkning.VedtakId,
        VilkarId = virkning.VilkarId,
        Type = virkning.Type,
        Fastsettelsesmate = virkning.Fastsettelsesmate,
        Beskrivelse = FlerspraakligTekstMapper.TilDto(virkning.BeskrivelseTekst),
        Varighet = virkning.Varighet,
        GyldigFra = virkning.GyldigFra,
        GyldigTil = virkning.GyldigTil,
        Belop = virkning.Belop,
        LopendeVilkar = FlerspraakligTekstMapper.TilDto(virkning.LopendeVilkarTekst),
        RapporteringsFrekvens = virkning.RapporteringsFrekvens,
        AvledetFraVirkningId = virkning.AvledetFraVirkningId,
        VurderingIder = virkning.VedtaksvirkningVurdering.Select(vv => vv.VurderingId).ToList(),
        FaktumIder = virkning.VedtaksvirkningFaktum.Select(vf => vf.FaktumId).ToList()
    };
}
