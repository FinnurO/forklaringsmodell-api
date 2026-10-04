using System.Net.Http.Json;

namespace Forklaringsmodell.Tests.Integration;

/// <summary>
/// Reelt demonstrasjonscase, utledet fra Stavanger kommunes automatiske godkjenning av
/// søknader om piperehabilitering (Norges første automatiske byggetillatelse, se
/// https://www.dibk.no/nyhetsarkiv/forste-steg-for-automatisk-byggesaksbehandling og
/// https://byggmesteren.as/2026/09/29/norges-forste-automatiske-byggetillatelse/).
///
/// Rettslig grunnlag: plan- og bygningsloven § 20-1 bokstav f (rehabilitering av skorstein
/// er søknadspliktig, se https://www.regjeringen.no/no/dokumenter/-20-bokstav-f---departementet-besvarer-henvendelse-om-rehabilitering-av-skorstein-piper/id2670833/)
/// og §§ 22-1/23-3 (ansvarlig foretak, sentral godkjenning for ansvarsrett).
///
/// Viser at forklaringsmodellen kan gjengi hele beslutningstreet bak en automatisk
/// godkjenning (regel 3.18, v1.6.0) — ikke bare hovedkonklusjonen — og at en sak som ikke
/// kvalifiserer for automatisering kan dokumenteres fullt ut selv uten noe Vedtak ennå
/// (regel 3.14).
/// </summary>
public class PiperehabiliteringScenarioTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PiperehabiliteringScenarioTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static List<TekstVariant> Tekst(params (string Kode, string Verdi)[] varianter) =>
        varianter.Select(v => new TekstVariant(v.Kode, v.Verdi)).ToList();

    [Fact]
    public async Task Piperehabilitering_med_alle_delvilkar_oppfylt_godkjennes_automatisk_med_fullt_vurderingstre()
    {
        var client = _factory.CreateClient();

        var rkSoknadsplikt = await client.PostAsJsonAsync("/api/rettskilder", new
        {
            type = "Lov",
            henvisning = "plan- og bygningsloven § 20-1 bokstav f",
            versjonDato = DateTimeOffset.UtcNow,
            eliReferanse = "https://lovdata.no/lov/2008-06-27-71"
        });
        var rkSoknadspliktId = (await rkSoknadsplikt.Content.ReadFromJsonAsync<RettskildeResult>())!.RettskildeId;

        var rkAnsvarsrett = await client.PostAsJsonAsync("/api/rettskilder", new
        {
            type = "Lov",
            henvisning = "plan- og bygningsloven § 23-3 jf. § 22-1 (sentral godkjenning)",
            versjonDato = DateTimeOffset.UtcNow,
            eliReferanse = "https://lovdata.no/lov/2008-06-27-71"
        });
        var rkAnsvarsrettId = (await rkAnsvarsrett.Content.ReadFromJsonAsync<RettskildeResult>())!.RettskildeId;

        var kildeSoknad = await client.PostAsJsonAsync("/api/kilder", new
        {
            navn = "eByggesøk Proff (søknadsportal)",
            type = "Soknad",
            autoritativ = false,
            rettskildeIder = new[] { rkSoknadspliktId }
        });
        var kildeSoknadId = (await kildeSoknad.Content.ReadFromJsonAsync<KildeResult>())!.KildeId;

        var kildeSentralGodkjenning = await client.PostAsJsonAsync("/api/kilder", new
        {
            navn = "Sentral godkjenning for ansvarsrett (DiBK-registeret)",
            type = "AutoritativtRegister",
            autoritativ = true,
            rettskildeIder = new[] { rkAnsvarsrettId }
        });
        var kildeSentralGodkjenningId = (await kildeSentralGodkjenning.Content.ReadFromJsonAsync<KildeResult>())!.KildeId;

        var regel = await client.PostAsJsonAsync("/api/regler", new
        {
            rettskildeIder = new[] { rkSoknadspliktId },
            teknologi = "DiBK nasjonal sjekkliste",
            type = "Deterministisk",
            regeldefinisjonReferanse = "https://sjekkliste-bygg-api.ft.dibk.no/api/sjekkliste"
        });
        var regelId = (await regel.Content.ReadFromJsonAsync<RegelResult>())!.RegelId;

        var vilkar = await client.PostAsJsonAsync("/api/vilkar", new
        {
            navn = "Automatisk godkjenning av piperehabilitering",
            kode = "PIPEREHAB_AUTOMATISK",
            kodeverk = "DIBK_SJEKKLISTE_BYGG",
            type = "Tillatelse",
            grunnlagstype = "Rettslig",
            fastsettelsesmate = "Parametrisert",
            rettskildeIder = new[] { rkSoknadspliktId },
            regelId,
            standardTekst = Tekst(("nb", "Automatisk godkjenning av piperehabilitering."), ("nn", "Automatisk godkjenning av piperehabilitering."))
        });
        var vilkarId = (await vilkar.Content.ReadFromJsonAsync<VilkarResult>())!.VilkarId;

        var sak = await client.PostAsJsonAsync("/api/saker", new
        {
            tittel = "Søknad om rehabilitering av skorstein – Eiganesveien 12, Stavanger",
            status = "UnderBehandling",
            utlosendeHendelse = "Soknad"
        });
        var sakId = (await sak.Content.ReadFromJsonAsync<SakResult>())!.SakId;

        var faktumOmfang = await client.PostAsJsonAsync($"/api/saker/{sakId}/faktum", new
        {
            kildeId = kildeSoknadId,
            type = "Raatt",
            struktur = "Strukturert",
            verdi = "Tiltaket er innvendig fôring av eksisterende skorstein. Ingen endring av utvendige dimensjoner."
        });
        var faktumOmfangId = (await faktumOmfang.Content.ReadFromJsonAsync<FaktumResult>())!.FaktumId;

        var faktumAnsvarsrett = await client.PostAsJsonAsync($"/api/saker/{sakId}/faktum", new
        {
            kildeId = kildeSentralGodkjenningId,
            type = "Raatt",
            struktur = "Strukturert",
            verdi = "Ansvarlig foretak har gyldig sentral godkjenning for ansvarsrett, tiltaksklasse 1, brannsikkerhet/skorstein."
        });
        var faktumAnsvarsrettId = (await faktumAnsvarsrett.Content.ReadFromJsonAsync<FaktumResult>())!.FaktumId;

        var vHoved = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "Oppfylt",
            beregningsspor = "Alle delvilkår i DiBKs sjekkliste er oppfylt => automatisk godkjent",
            eskalert = false,
            vilkarId
        });
        var vHovedId = (await vHoved.Content.ReadFromJsonAsync<VurderingResult>())!.VurderingId;

        var vAnsvarsrett = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "Oppfylt",
            beregningsspor = "Ansvarlig foretak har gyldig sentral godkjenning for ansvarsrett => vilkår oppfylt",
            eskalert = false,
            vilkarId,
            forelderVurderingId = vHovedId,
            faktumIder = new[] { faktumAnsvarsrettId },
            rettskildeIder = new[] { rkAnsvarsrettId }
        });
        var vAnsvarsrettId = (await vAnsvarsrett.Content.ReadFromJsonAsync<VurderingResult>())!.VurderingId;

        var vOmfang = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "Oppfylt",
            beregningsspor = "Tiltaket er begrenset til innvendig fôring => regnes som rehabilitering, ikke ny installasjon",
            eskalert = false,
            vilkarId,
            forelderVurderingId = vHovedId,
            faktumIder = new[] { faktumOmfangId }
        });
        var vOmfangId = (await vOmfang.Content.ReadFromJsonAsync<VurderingResult>())!.VurderingId;

        var faktumFredning = await client.PostAsJsonAsync($"/api/saker/{sakId}/faktum", new
        {
            kildeId = kildeSoknadId,
            type = "Raatt",
            struktur = "Strukturert",
            verdi = "Bygningen er ikke registrert som fredet eller verneverdig i kommunens kulturminneregister."
        });
        var faktumFredningId = (await faktumFredning.Content.ReadFromJsonAsync<FaktumResult>())!.FaktumId;

        var vFredning = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "Oppfylt",
            beregningsspor = "Bygningen er ikke fredet eller verneverdig => ingen tilleggsvilkår utløses",
            eskalert = false,
            vilkarId,
            forelderVurderingId = vHovedId,
            faktumIder = new[] { faktumFredningId }
        });
        var vFredningId = (await vFredning.Content.ReadFromJsonAsync<VurderingResult>())!.VurderingId;

        var vedtakResponse = await client.PostAsJsonAsync($"/api/saker/{sakId}/vedtak", new
        {
            utfall = "Tillatelse til rehabilitering av skorstein innvilget",
            faktumIder = new[] { faktumOmfangId, faktumAnsvarsrettId, faktumFredningId },
            vurderingIder = new[] { vHovedId, vAnsvarsrettId, vOmfangId, vFredningId },
            partsmedvirkningIder = Array.Empty<Guid>(),
            virkninger = new[]
            {
                new
                {
                    type = "Tillatelse",
                    beskrivelse = Tekst(("nb", "Tillatelse til rehabilitering av skorstein (innvendig fôring)"), ("nn", "Løyve til rehabilitering av skorstein (innvendig fôring)")),
                    fastsettelsesmate = "Parametrisert",
                    vilkarId,
                    varighet = "Varig",
                    vurderingIder = new[] { vHovedId },
                    faktumIder = new[] { faktumOmfangId }
                }
            }
        });
        vedtakResponse.EnsureSuccessStatusCode();
        var vedtak = await vedtakResponse.Content.ReadFromJsonAsync<VedtakResult>();

        // Regel 3.5: kun deterministiske, ikke-eskalerte vurderinger => Helautomatisert,
        // akkurat som i det virkelige Stavanger-caset.
        Assert.Equal("Helautomatisert", vedtak!.AutomatiseringsGrad);

        var forklaringResponse = await client.GetAsync($"/api/vedtak/{vedtak.VedtakId}/forklaring");
        forklaringResponse.EnsureSuccessStatusCode();
        var forklaring = await forklaringResponse.Content.ReadFromJsonAsync<ForklaringResult>();

        Assert.Equal(4, forklaring!.Vurderinger.Count);
        var hoved = forklaring.Vurderinger.Single(v => v.VurderingId == vHovedId);
        Assert.Equal(3, hoved.DelvurderingIder.Count);
        Assert.Contains(vAnsvarsrettId, hoved.DelvurderingIder);
        Assert.Contains(vOmfangId, hoved.DelvurderingIder);
        Assert.Contains(vFredningId, hoved.DelvurderingIder);
        Assert.All(forklaring.Vurderinger, v => Assert.Equal(vilkarId, v.VilkarId));

        var virkning = forklaring.Virkninger.Single();
        Assert.Contains(virkning.Beskrivelse, v => v.SpraakKode == "nb");
        Assert.Contains(virkning.Beskrivelse, v => v.SpraakKode == "nn");
    }

    [Fact]
    public async Task Piperehabilitering_med_mangelfull_ansvarsrett_eskaleres_til_manuell_uten_vedtak()
    {
        var client = _factory.CreateClient();

        var rettskilde = await client.PostAsJsonAsync("/api/rettskilder", new
        {
            type = "Lov",
            henvisning = "plan- og bygningsloven § 20-1 bokstav f",
            versjonDato = DateTimeOffset.UtcNow,
            eliReferanse = "https://lovdata.no/lov/2008-06-27-71"
        });
        var rettskildeId = (await rettskilde.Content.ReadFromJsonAsync<RettskildeResult>())!.RettskildeId;

        var kilde = await client.PostAsJsonAsync("/api/kilder", new
        {
            navn = "eByggesøk Proff (søknadsportal)",
            type = "Soknad",
            autoritativ = false,
            rettskildeIder = new[] { rettskildeId }
        });
        var kildeId = (await kilde.Content.ReadFromJsonAsync<KildeResult>())!.KildeId;

        var regel = await client.PostAsJsonAsync("/api/regler", new
        {
            rettskildeIder = new[] { rettskildeId },
            teknologi = "DiBK nasjonal sjekkliste",
            type = "Deterministisk"
        });
        var regelId = (await regel.Content.ReadFromJsonAsync<RegelResult>())!.RegelId;

        var sak = await client.PostAsJsonAsync("/api/saker", new
        {
            tittel = "Søknad om rehabilitering av skorstein – Hinna Park 4, Stavanger",
            status = "UnderBehandling",
            utlosendeHendelse = "Soknad"
        });
        var sakId = (await sak.Content.ReadFromJsonAsync<SakResult>())!.SakId;

        var faktumAnsvarsrett = await client.PostAsJsonAsync($"/api/saker/{sakId}/faktum", new
        {
            kildeId,
            type = "Raatt",
            struktur = "Strukturert",
            verdi = "Ansvarlig foretak har sentral godkjenning kun for tiltaksklasse 1 ildsted – dekker ikke kategorien dette tiltaket krever."
        });
        var faktumAnsvarsrettId = (await faktumAnsvarsrett.Content.ReadFromJsonAsync<FaktumResult>())!.FaktumId;

        var vHoved = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "Uavklart",
            beregningsspor = "Ett av delvilkårene (ansvarsrett) kunne ikke bekreftes automatisk => rutes til manuell saksbehandling",
            eskalert = true
        });
        var vHovedId = (await vHoved.Content.ReadFromJsonAsync<VurderingResult>())!.VurderingId;

        var vAnsvarsrett = await client.PostAsJsonAsync($"/api/saker/{sakId}/vurderinger", new
        {
            regelId,
            type = "Deterministisk",
            utfall = "IkkeOppfylt",
            beregningsspor = "Foretakets sentrale godkjenning dekker ikke riktig tiltaksklasse => kan ikke automatisk bekreftes",
            eskalert = true,
            forelderVurderingId = vHovedId,
            faktumIder = new[] { faktumAnsvarsrettId }
        });
        var vAnsvarsrettResult = await vAnsvarsrett.Content.ReadFromJsonAsync<VurderingResult>();

        // Ingen Vedtak er opprettet ennå — saken er fortsatt under (manuell) behandling,
        // men forklaringen for hvorfor den ikke kvalifiserte for automatisering er
        // allerede fullt dokumentert (regel 3.14).
        var vurderingerResponse = await client.GetAsync($"/api/saker/{sakId}/vurderinger");
        vurderingerResponse.EnsureSuccessStatusCode();
        var vurderinger = await vurderingerResponse.Content.ReadFromJsonAsync<List<VurderingResult>>();

        var hoved = vurderinger!.Single(v => v.VurderingId == vHovedId);
        Assert.Equal("Uavklart", hoved.Utfall);
        Assert.True(hoved.Eskalert);
        Assert.Contains(vAnsvarsrettResult!.VurderingId, hoved.DelvurderingIder);
        Assert.False(hoved.ErLaast);

        var vedtakResponse = await client.GetAsync($"/api/saker/{sakId}/vedtak");
        vedtakResponse.EnsureSuccessStatusCode();
        var vedtakListe = await vedtakResponse.Content.ReadFromJsonAsync<List<object>>();
        Assert.Empty(vedtakListe!);
    }

    private record TekstVariant(string SpraakKode, string Verdi);
    private record RettskildeResult(Guid RettskildeId);
    private record KildeResult(Guid KildeId);
    private record RegelResult(Guid RegelId);
    private record VilkarResult(Guid VilkarId);
    private record SakResult(Guid SakId);
    private record FaktumResult(Guid FaktumId);
    private record VurderingResult(Guid VurderingId, string Utfall, bool Eskalert, bool ErLaast, Guid? VilkarId, List<Guid> DelvurderingIder);
    private record VedtakResult(Guid VedtakId, string AutomatiseringsGrad);
    private record VedtaksvirkningResult(Guid VirkningId, List<TekstVariant> Beskrivelse);
    private record ForklaringResult(List<VurderingResult> Vurderinger, List<VedtaksvirkningResult> Virkninger);
}
