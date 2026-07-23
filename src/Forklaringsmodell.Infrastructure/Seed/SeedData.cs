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
            CpsvTjenesteReferanse = "https://data.norge.no/services/dagpenger",
            UtlosendeHendelse = HendelseType.Soknad
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
            CccevReferanse = "https://data.norge.no/evidences/inntektsopplysninger"
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
            Type = VurderingsType.Deterministisk,
            RegeldefinisjonReferanse = "https://regelrepo.example.test/dagpenger/inntektskrav/v3.dmn"
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
            Utfall = UtfallType.Oppfylt,
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
            Utfall = UtfallType.Uavklart,
            Konfidens = 0.62m,
            Eskalert = true,
            Beregningsspor = "klassifisert som 'uklar', under terskel 0,80 => eskalert til skjønn"
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
            Utfall = UtfallType.Oppfylt,
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

        // Vilkar-katalogoppføring (regel 3.12) som dagpenger-virkningen er en instans av.
        var vilkarDagpengesats = new Vilkar
        {
            VilkarId = Guid.NewGuid(),
            Navn = "Dagpengesats basert på tidligere inntekt",
            Kode = "DP_SATS_INNTEKT",
            Kodeverk = "NAV_VILKAR_TYPE",
            Type = VirkningType.OkonomiskYtelse,
            Grunnlagstype = GrunnlagsType.Rettslig,
            Fastsettelsesmate = FastsettelsesmateType.Parametrisert,
            StandardTekst = "Dagpenger utbetales med en sats beregnet av tidligere inntekt, jf. folketrygdloven § 4-5."
        };
        vilkarDagpengesats.VilkarRettskilde.Add(new VilkarRettskilde { VilkarId = vilkarDagpengesats.VilkarId, RettskildeId = rettskildeInntektskrav.RettskildeId });

        // Regel 3.15: et Vilkar med Grunnlagstype == Datakvalitet er en teknisk kontroll,
        // ikke et rettslig krav, og har derfor bevisst ingen RettskildeIder (jf. DUF-
        // aliaseksempelet i spesifikasjonens punkt 6).
        var vilkarDatakvalitetKontroll = new Vilkar
        {
            VilkarId = Guid.NewGuid(),
            Navn = "Identitet er ikke dobbeltregistrert",
            Kode = "DP_IDENTITET_UNIK",
            Kodeverk = "NAV_VILKAR_TYPE",
            Type = VirkningType.Tillatelse,
            Grunnlagstype = GrunnlagsType.Datakvalitet,
            Fastsettelsesmate = FastsettelsesmateType.Statisk,
            StandardTekst = "Kontrollerer at søkerens identitet ikke er registrert som alias av en annen identitet."
        };

        // Vedtaksvirkning fra "Body for POST .../vedtak"-eksempelet i spesifikasjonens
        // punkt 5 (regel 3.10): den økonomiske ytelsen dagpenger-vedtaket faktisk medfører.
        var virkningDagpenger = new Vedtaksvirkning
        {
            VirkningId = Guid.NewGuid(),
            VedtakId = vedtak.VedtakId,
            VilkarId = vilkarDagpengesats.VilkarId,
            Type = VirkningType.OkonomiskYtelse,
            Fastsettelsesmate = FastsettelsesmateType.Parametrisert,
            Beskrivelse = "Dagpenger",
            Varighet = VarighetsType.Tidsbegrenset,
            GyldigFra = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            GyldigTil = new DateTimeOffset(2027, 1, 31, 0, 0, 0, TimeSpan.Zero),
            Belop = 18500m
        };
        virkningDagpenger.VedtaksvirkningVurdering.Add(new VedtaksvirkningVurdering { VirkningId = virkningDagpenger.VirkningId, VurderingId = vurderingDeterministisk.VurderingId });
        virkningDagpenger.VedtaksvirkningFaktum.Add(new VedtaksvirkningFaktum { VirkningId = virkningDagpenger.VirkningId, FaktumId = faktumInntekt.FaktumId });

        // Regel 3.14: to ureferert (ikke-frosne) Vurdering-rader som demonstrerer at et
        // vilkår kan dokumenteres som ikke faktisk vurdert, uten at fraværet av en rad er
        // den eneste dokumentasjonen — årsaken fremgår av Beregningsspor.
        var vurderingUaktuelt = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regelDeterministisk.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Uaktuelt,
            Beregningsspor = "Uaktuelt: saken gjelder førstegangssøknad, ikke fornyelse av tidligere dagpengeperiode",
            Eskalert = false
        };

        var vurderingIkkeVurdert = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sak.SakId,
            RegelId = regelDeterministisk.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.IkkeVurdert,
            Beregningsspor = "IkkeVurdert: behandlingen stoppet ved inntektsvilkåret, som allerede avgjorde utfallet",
            Eskalert = false
        };

        db.Vilkar.AddRange(vilkarDagpengesats, vilkarDatakvalitetKontroll);
        db.Saker.Add(sak);
        db.Rettskilder.AddRange(rettskildeInntektskrav, rettskildeRundskriv, rettskildeInnhenting, rettskildeOpplysningsplikt);
        db.Kilder.AddRange(kildeAOrdningen, kildeSoknad);
        db.Faktum.AddRange(faktumInntekt, faktumBegrunnelse);
        db.Regler.AddRange(regelDeterministisk, regelGenerativKI, regelSkjonn);
        db.Vurderinger.AddRange(vurderingDeterministisk, vurderingGenerativKI, vurderingSkjonn, vurderingUaktuelt, vurderingIkkeVurdert);
        db.Vedtak.Add(vedtak);
        db.Forklaringslogger.Add(logg);
        db.Vedtaksvirkninger.Add(virkningDagpenger);

        // Vedtaket (og dermed vurderingSkjonn) må være lagret og frosset før sak 2 kan
        // kryss-sak-referere til det (regel 3.11), så denne lagres i et eget kall.
        await db.SaveChangesAsync(ct);

        // Tilleggseksempel fra spesifikasjonens punkt 6: melding om endret inntekt utløser
        // en ny Sak som følger opp den opprinnelige via SakRelasjon, og gjenbruker den
        // opprinnelige (nå frosne) skjønnsvurderingen av oppsigelsesgrunn via
        // RefererteVurderingIder i stedet for å vurdere den på nytt.
        var sakMelding = new Sak
        {
            SakId = Guid.NewGuid(),
            Tittel = "Melding om endret inntekt",
            Status = SakStatus.UnderBehandling,
            Opprettet = naa,
            SistEndret = naa,
            UtlosendeHendelse = HendelseType.Melding
        };

        var relasjonTilOpprinneligSak = new SakRelasjon
        {
            RelasjonId = Guid.NewGuid(),
            SakId = sakMelding.SakId,
            RelatertSakId = sak.SakId,
            Type = SakRelasjonType.OppfolgingAvMelding
        };

        var faktumNyInntekt = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sakMelding.SakId,
            KildeId = kildeSoknad.KildeId,
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Strukturert,
            Verdi = "480000",
            InnhentetTidspunkt = naa
        };

        var vurderingRevurdertInntekt = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = sakMelding.SakId,
            RegelId = regelDeterministisk.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt,
            Beregningsspor = "revurdert inntekt >= 1.5G => fortsatt oppfylt",
            Eskalert = false
        };
        vurderingRevurdertInntekt.VurderingFaktum.Add(new VurderingFaktum
        {
            VurderingId = vurderingRevurdertInntekt.VurderingId,
            FaktumId = faktumNyInntekt.FaktumId
        });
        vurderingRevurdertInntekt.RefererteVurderinger.Add(new VurderingReferanse
        {
            VurderingId = vurderingRevurdertInntekt.VurderingId,
            RefererteVurderingId = vurderingSkjonn.VurderingId
        });

        // Andre vedtak (på sakMelding) demonstrerer Vedtaksvirkning.AvledetFraVirkningId
        // (regel 3.13): en justert dagpengesats avledet fra den opprinnelige virkningen på
        // det første, allerede frosne vedtaket — på tvers av både Vedtak og Sak.
        var vedtakMelding = new Vedtak
        {
            VedtakId = Guid.NewGuid(),
            SakId = sakMelding.SakId,
            Tidspunkt = naa,
            Utfall = "Dagpengesats justert etter endret inntekt",
            AutomatiseringsGrad = AutomatiseringsGrad.Helautomatisert
        };

        var loggMelding = new Forklaringslogg
        {
            LoggId = Guid.NewGuid(),
            VedtakId = vedtakMelding.VedtakId
        };
        loggMelding.Oppforinger.Add(new ForklaringsloggOppforing
        {
            OppforingId = Guid.NewGuid(),
            LoggId = loggMelding.LoggId,
            Type = OppforingsType.Faktum,
            ReferanseId = faktumNyInntekt.FaktumId
        });
        loggMelding.Oppforinger.Add(new ForklaringsloggOppforing
        {
            OppforingId = Guid.NewGuid(),
            LoggId = loggMelding.LoggId,
            Type = OppforingsType.Vurdering,
            ReferanseId = vurderingRevurdertInntekt.VurderingId
        });

        var virkningJustertSats = new Vedtaksvirkning
        {
            VirkningId = Guid.NewGuid(),
            VedtakId = vedtakMelding.VedtakId,
            VilkarId = vilkarDagpengesats.VilkarId,
            Type = VirkningType.OkonomiskYtelse,
            Fastsettelsesmate = FastsettelsesmateType.Avledet,
            Beskrivelse = "Justert dagpengesats etter endret inntekt",
            Varighet = VarighetsType.Tidsbegrenset,
            GyldigFra = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
            GyldigTil = new DateTimeOffset(2027, 1, 31, 0, 0, 0, TimeSpan.Zero),
            Belop = 21000m,
            AvledetFraVirkningId = virkningDagpenger.VirkningId
        };
        virkningJustertSats.VedtaksvirkningVurdering.Add(new VedtaksvirkningVurdering { VirkningId = virkningJustertSats.VirkningId, VurderingId = vurderingRevurdertInntekt.VurderingId });
        virkningJustertSats.VedtaksvirkningFaktum.Add(new VedtaksvirkningFaktum { VirkningId = virkningJustertSats.VirkningId, FaktumId = faktumNyInntekt.FaktumId });

        db.Saker.Add(sakMelding);
        db.SakRelasjoner.Add(relasjonTilOpprinneligSak);
        db.Faktum.Add(faktumNyInntekt);
        db.Vurderinger.Add(vurderingRevurdertInntekt);
        db.Vedtak.Add(vedtakMelding);
        db.Forklaringslogger.Add(loggMelding);
        db.Vedtaksvirkninger.Add(virkningJustertSats);

        await db.SaveChangesAsync(ct);
    }
}
