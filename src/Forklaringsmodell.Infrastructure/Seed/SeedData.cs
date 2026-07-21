using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Forklaringsmodell.Infrastructure.Seed;

/// <summary>
/// Setter inn eksempeldataene fra spesifikasjonens punkt 6 (dagpenger-eksempelet), for
/// manuell verifisering. Kjøres fra Program.cs i utviklingsmiljø hvis databasen er tom
/// (se sjekk på !Saker.Any() før innsetting).
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(ForklaringsmodellDbContext db, CancellationToken ct = default)
    {
        if (await db.Saker.AnyAsync(ct))
        {
            return; // allerede seedet
        }

        var naa = DateTimeOffset.UtcNow;

        var sak = new Sak
        {
            SakId = Guid.NewGuid(),
            Tittel = "Søknad om dagpenger",
            Status = SakStatus.UnderBehandling,
            Opprettet = naa,
            SistEndret = naa,
            TjenesteReferanse = "https://data.norge.no/services/dagpenger"
        };

        // De tre rettskildene fra spesifikasjonens punkt 6.
        var rettskildeInntektskrav = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Type = RettskildeType.Lov,
            Henvisning = "folketrygdloven § 4-5"
        };

        var rettskildeRundskriv = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Type = RettskildeType.Rundskriv,
            Henvisning = "NAV rundskriv til § 4-5, pkt. 4.5.3 (selvforskyldt oppsigelse)"
        };

        var rettskildeInnhenting = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Type = RettskildeType.Lov,
            Henvisning = "folketrygdloven § 21-4 (innhenting av opplysninger)"
        };

        // Spesifikasjonens eksempel viser ingen rettskildereferanse for "Søknad"-kilden,
        // men regel 3.8 krever at enhver Kilde har minst én hjemmel for innhenting. Fyller
        // derfor inn en fjerde, plausibel rettskilde for å holde seed-dataen internt
        // konsistent med forretningsregelen (pragmatisk utfylling av et gap i eksempelet).
        var rettskildeOpplysningsplikt = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Type = RettskildeType.Lov,
            Henvisning = "folketrygdloven § 21-3 (søkers opplysningsplikt)"
        };

        var kildeAOrdningen = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = "A-ordningen",
            Type = KildeType.AutoritativtRegister,
            Autoritativ = true,
            CpsvReferanse = "https://data.norge.no/evidences/inntektsopplysninger"
        };
        kildeAOrdningen.KildeRettskilde.Add(new KildeRettskilde { KildeId = kildeAOrdningen.KildeId, RettskildeId = rettskildeInnhenting.RettskildeId });

        var kildeSoknad = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = "Søknad",
            Type = KildeType.Soknad,
            Autoritativ = false
        };
        kildeSoknad.KildeRettskilde.Add(new KildeRettskilde { KildeId = kildeSoknad.KildeId, RettskildeId = rettskildeOpplysningsplikt.RettskildeId });

        var faktumInntekt = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sak.SakId,
            KildeId = kildeAOrdningen.KildeId,
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Strukturert,
            Verdi = "420000",
            InnhentetTidspunkt = naa
        };

        var faktumBegrunnelse = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sak.SakId,
            KildeId = kildeSoknad.KildeId,
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Ustrukturert,
            Verdi = "Fikk ikke fornyet vikariat, arbeidsgiver nedbemannet",
            InnhentetTidspunkt = naa
        };

        // Regel-rader per vurderingstype, hjemlet i den generelle rettskilden som best
        // matcher spesifikasjonens eksempel (regel 3.7).
        var regelDeterministisk = new Regel
        {
            RegelId = Guid.NewGuid(),
            Teknologi = "DMN",
            Type = VurderingsType.Deterministisk
        };
        regelDeterministisk.RegelRettskilde.Add(new RegelRettskilde { RegelId = regelDeterministisk.RegelId, RettskildeId = rettskildeInntektskrav.RettskildeId });

        var regelGenerativKI = new Regel
        {
            RegelId = Guid.NewGuid(),
            Teknologi = "LLM-prompt v3",
            Type = VurderingsType.GenerativKI
        };
        regelGenerativKI.RegelRettskilde.Add(new RegelRettskilde { RegelId = regelGenerativKI.RegelId, RettskildeId = rettskildeInntektskrav.RettskildeId });

        var regelSkjonn = new Regel
        {
            RegelId = Guid.NewGuid(),
            Teknologi = "Saksbehandler",
            Type = VurderingsType.Skjonn
        };
        regelSkjonn.RegelRettskilde.Add(new RegelRettskilde { RegelId = regelSkjonn.RegelId, RettskildeId = rettskildeRundskriv.RettskildeId });

        var vurderingDeterministisk = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regelDeterministisk.RegelId,
            Type = VurderingsType.Deterministisk,
            Beregningsspor = "inntekt >= 1.5G => oppfylt",
            Eskalert = false
        };
        vurderingDeterministisk.VurderingFaktum.Add(new VurderingFaktum
        {
            VurderingId = vurderingDeterministisk.VurderingId,
            FaktumId = faktumInntekt.FaktumId
        });
        vurderingDeterministisk.VurderingRettskilde.Add(new VurderingRettskilde
        {
            VurderingId = vurderingDeterministisk.VurderingId,
            RettskildeId = rettskildeInntektskrav.RettskildeId
        });

        var vurderingGenerativKI = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regelGenerativKI.RegelId,
            Type = VurderingsType.GenerativKI,
            Konfidens = 0.62m,
            Eskalert = true,
            Beregningsspor = "klassifisert som 'uklar'"
        };
        vurderingGenerativKI.VurderingFaktum.Add(new VurderingFaktum
        {
            VurderingId = vurderingGenerativKI.VurderingId,
            FaktumId = faktumBegrunnelse.FaktumId
        });

        var vurderingSkjonn = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regelSkjonn.RegelId,
            Type = VurderingsType.Skjonn,
            Hovedhensyn = "Dokumentert nedbemanning hos arbeidsgiver",
            ForkastedeUtfall = "Selvforskyldt oppsigelse",
            Eskalert = false
        };
        vurderingSkjonn.VurderingFaktum.Add(new VurderingFaktum
        {
            VurderingId = vurderingSkjonn.VurderingId,
            FaktumId = faktumBegrunnelse.FaktumId
        });
        vurderingSkjonn.VurderingRettskilde.Add(new VurderingRettskilde
        {
            VurderingId = vurderingSkjonn.VurderingId,
            RettskildeId = rettskildeRundskriv.RettskildeId
        });

        // Regel 3.5: AutomatiseringsGrad beregnes ut fra andelen Skjonn/eskalerte
        // vurderinger. Her: 3 vurderinger, 2 er skjønn/eskalert (KI-en er eskalert,
        // skjønnsvurderingen er skjønn) => DelvisAutomatisert, i tråd med eksempeldataen
        // i spesifikasjonens punkt 6.
        var vedtak = new Vedtak
        {
            VedtakId = Guid.NewGuid(),
            SakId = sak.SakId,
            Tidspunkt = naa,
            Utfall = "Dagpenger tilkjent",
            AutomatiseringsGrad = AutomatiseringsGrad.DelvisAutomatisert
        };

        var logg = new Forklaringslogg
        {
            LoggId = Guid.NewGuid(),
            VedtakId = vedtak.VedtakId
        };

        void LeggTilOppforing(OppforingsType type, Guid referanseId) =>
            logg.Oppforinger.Add(new ForklaringsloggOppforing
            {
                OppforingId = Guid.NewGuid(),
                LoggId = logg.LoggId,
                Type = type,
                ReferanseId = referanseId
            });

        LeggTilOppforing(OppforingsType.Faktum, faktumInntekt.FaktumId);
        LeggTilOppforing(OppforingsType.Faktum, faktumBegrunnelse.FaktumId);
        LeggTilOppforing(OppforingsType.Vurdering, vurderingDeterministisk.VurderingId);
        LeggTilOppforing(OppforingsType.Vurdering, vurderingGenerativKI.VurderingId);
        LeggTilOppforing(OppforingsType.Vurdering, vurderingSkjonn.VurderingId);

        db.Saker.Add(sak);
        db.Rettskilder.AddRange(rettskildeInntektskrav, rettskildeRundskriv, rettskildeInnhenting, rettskildeOpplysningsplikt);
        db.Kilder.AddRange(kildeAOrdningen, kildeSoknad);
        db.Faktum.AddRange(faktumInntekt, faktumBegrunnelse);
        db.Regler.AddRange(regelDeterministisk, regelGenerativKI, regelSkjonn);
        db.Vurderinger.AddRange(vurderingDeterministisk, vurderingGenerativKI, vurderingSkjonn);
        db.Vedtak.Add(vedtak);
        db.Forklaringslogger.Add(logg);

        await db.SaveChangesAsync(ct);
    }
}
