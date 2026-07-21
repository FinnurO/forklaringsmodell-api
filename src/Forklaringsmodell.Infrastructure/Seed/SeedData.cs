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
            SistEndret = naa
        };

        var kildeAOrdningen = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = "A-ordningen",
            Type = KildeType.AutoritativtRegister,
            Autoritativ = true
        };

        var kildeSoknad = new Kilde
        {
            KildeId = Guid.NewGuid(),
            Navn = "Søknad",
            Type = KildeType.Soknad,
            Autoritativ = false
        };

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

        // En Rettskilde/Regel per vurderingstype, som instruert i spesifikasjonens punkt 6.
        var rettskildeDeterministisk = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Paragraf = "Folketrygdloven § 4-4 (inntektskrav)",
            VersjonDato = naa,
            EliReferanse = "https://lovdata.no/nav/folketrygdloven/kap4/section4-4"
        };
        var regelDeterministisk = new Regel
        {
            RegelId = Guid.NewGuid(),
            RettskildeId = rettskildeDeterministisk.RettskildeId,
            Teknologi = "DMN",
            Type = VurderingsType.Deterministisk
        };

        var rettskildeGenerativKI = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Paragraf = "Folketrygdloven § 4-3 (klassifisering av oppsigelsesgrunn)",
            VersjonDato = naa,
            EliReferanse = "https://lovdata.no/nav/folketrygdloven/kap4/section4-3"
        };
        var regelGenerativKI = new Regel
        {
            RegelId = Guid.NewGuid(),
            RettskildeId = rettskildeGenerativKI.RettskildeId,
            Teknologi = "LLM-prompt v3",
            Type = VurderingsType.GenerativKI
        };

        var rettskildeSkjonn = new Rettskilde
        {
            RettskildeId = Guid.NewGuid(),
            Paragraf = "Folketrygdloven § 4-10 (selvforskyldt ledighet)",
            VersjonDato = naa,
            EliReferanse = "https://lovdata.no/nav/folketrygdloven/kap4/section4-10"
        };
        var regelSkjonn = new Regel
        {
            RegelId = Guid.NewGuid(),
            RettskildeId = rettskildeSkjonn.RettskildeId,
            Teknologi = "Saksbehandler",
            Type = VurderingsType.Skjonn
        };

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
        db.Kilder.AddRange(kildeAOrdningen, kildeSoknad);
        db.Faktum.AddRange(faktumInntekt, faktumBegrunnelse);
        db.Rettskilder.AddRange(rettskildeDeterministisk, rettskildeGenerativKI, rettskildeSkjonn);
        db.Regler.AddRange(regelDeterministisk, regelGenerativKI, regelSkjonn);
        db.Vurderinger.AddRange(vurderingDeterministisk, vurderingGenerativKI, vurderingSkjonn);
        db.Vedtak.Add(vedtak);
        db.Forklaringslogger.Add(logg);

        await db.SaveChangesAsync(ct);
    }
}
