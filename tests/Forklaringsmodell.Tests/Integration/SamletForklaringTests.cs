using System.Net;
using System.Net.Http.Json;

namespace Forklaringsmodell.Tests.Integration;

/// <summary>
/// v1.7.0: forklaringen settes sammen i ett svar. Testene bruker seed-dataen (dagpenger-saken
/// og oppfølgingssaken) i utviklingsmiljø, pluss en egen sak uten vedtak.
/// </summary>
public class SamletForklaringTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SamletForklaringTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<Guid> SakIdAsync(HttpClient client, string tittel)
    {
        var saker = await client.GetFromJsonAsync<List<SakRef>>("/api/saker");
        return saker!.Single(s => s.Tittel == tittel).SakId;
    }

    private async Task<SakForklaring> HentSakForklaringAsync(HttpClient client, Guid sakId)
    {
        var response = await client.GetAsync($"/api/saker/{sakId}/forklaring");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SakForklaring>())!;
    }

    [Fact]
    public async Task Vedtak_forklaring_har_nostet_tre_og_oppløst_referansedata()
    {
        var client = _factory.CreateClient();
        var sakId = await SakIdAsync(client, "Søknad om dagpenger");
        var vedtak = await client.GetFromJsonAsync<List<VedtakRef>>($"/api/saker/{sakId}/vedtak");

        var forklaring = await client.GetFromJsonAsync<VedtakForklaring>($"/api/vedtak/{vedtak!.Single().VedtakId}/forklaring");

        // Bakoverkompatibelt: den flate listen finnes fortsatt, med de fire vurderingene i loggen.
        Assert.Equal(4, forklaring!.Vurderinger.Count);

        // Treet: tre røtter (deterministisk, KI, skjønn), og den deterministiske har én delvurdering.
        Assert.Equal(3, forklaring.Vurderingstre.Count);
        var rotMedBarn = Assert.Single(forklaring.Vurderingstre, n => n.Delvurderinger.Count > 0);
        Assert.Single(rotMedBarn.Delvurderinger);
        Assert.Equal(rotMedBarn.VurderingId, rotMedBarn.Delvurderinger[0].ForelderVurderingId);

        // Referansedata er oppløst, uten egne oppslag.
        Assert.Contains(forklaring.Referansedata.Kilder, k => k.Navn == "A-ordningen");
        Assert.Contains(forklaring.Referansedata.Vilkar, v => v.Kode == "DP_SATS_INNTEKT");
        Assert.Equal(3, forklaring.Referansedata.Regler.Count);
        Assert.NotEmpty(forklaring.Referansedata.Rettskilder);
        Assert.All(forklaring.Vurderinger, v => Assert.Contains(forklaring.Referansedata.Regler, r => r.RegelId == v.RegelId));
    }

    [Fact]
    public async Task Sak_forklaring_viser_hele_saken_og_skiller_frosset_fra_levende()
    {
        var client = _factory.CreateClient();
        var sakId = await SakIdAsync(client, "Søknad om dagpenger");

        var forklaring = await HentSakForklaringAsync(client, sakId);

        Assert.Equal(sakId, forklaring.Sak.SakId);
        Assert.True(forklaring.Oppsummering.HarVedtak);
        Assert.Equal(1, forklaring.Oppsummering.AntallVedtak);
        Assert.Equal(forklaring.Vurderinger.Count, forklaring.Oppsummering.AntallVurderinger);
        Assert.Single(forklaring.Vedtak);
        Assert.NotEmpty(forklaring.Vedtak[0].Virkninger);
        Assert.NotEmpty(forklaring.Vedtak[0].Forklaringslogg.Oppforinger);

        // Seed-saken har fire vurderinger i den frosne loggen og to som ikke er referert (regel 3.14).
        Assert.Equal(6, forklaring.Vurderinger.Count);
        Assert.Equal(4, forklaring.Vurderinger.Count(v => v.ErLaast));
        Assert.Equal(2, forklaring.Vurderinger.Count(v => !v.ErLaast));
        Assert.All(forklaring.Faktum, f => Assert.True(f.ErLaast));
    }

    [Fact]
    public async Task Sak_forklaring_folger_kryss_sak_referanser_ett_niva()
    {
        var client = _factory.CreateClient();
        var sakId = await SakIdAsync(client, "Melding om endret inntekt");

        var forklaring = await HentSakForklaringAsync(client, sakId);

        // Revurderingen siterer skjønnsvurderingen i den opprinnelige, frosne saken (regel 3.11) ...
        var referert = Assert.Single(forklaring.RefererteVurderinger);
        Assert.NotEqual(sakId, referert.SakId);
        Assert.True(referert.ErLaast);
        Assert.Single(forklaring.Vurderinger.Single().RefererteVurderingIder, referert.VurderingId);

        // ... og faktumet den siterte vurderingen bygger på hører til den andre saken.
        Assert.Contains(forklaring.AndreFaktum, f => f.SakId == referert.SakId);
        Assert.DoesNotContain(forklaring.Faktum, f => f.SakId != sakId);
    }

    [Fact]
    public async Task Sak_uten_vedtak_kan_forklares_og_er_ikke_frosset()
    {
        var client = _factory.CreateClient();

        var rettskilde = await (await client.PostAsJsonAsync("/api/rettskilder", new { type = "Lov", henvisning = "Test" })).Content.ReadFromJsonAsync<IdResult>();
        var kilde = await (await client.PostAsJsonAsync("/api/kilder", new { navn = "Test-kilde", type = "AnnenKilde", autoritativ = false, rettskildeIder = new[] { rettskilde!.RettskildeId } })).Content.ReadFromJsonAsync<IdResult>();
        var regel = await (await client.PostAsJsonAsync("/api/regler", new { rettskildeIder = new[] { rettskilde.RettskildeId }, teknologi = "Test", type = "Deterministisk" })).Content.ReadFromJsonAsync<IdResult>();
        var sak = await (await client.PostAsJsonAsync("/api/saker", new { tittel = "Sak uten vedtak", status = "UnderBehandling", utlosendeHendelse = "Soknad" })).Content.ReadFromJsonAsync<IdResult>();
        var faktum = await (await client.PostAsJsonAsync($"/api/saker/{sak!.SakId}/faktum", new { kildeId = kilde!.KildeId, type = "Raatt", struktur = "Ustrukturert", verdi = "Et faktum" })).Content.ReadFromJsonAsync<IdResult>();

        var rot = await (await client.PostAsJsonAsync($"/api/saker/{sak.SakId}/vurderinger", new { regelId = regel!.RegelId, type = "Deterministisk", utfall = "Uavklart", eskalert = true })).Content.ReadFromJsonAsync<IdResult>();
        await client.PostAsJsonAsync($"/api/saker/{sak.SakId}/vurderinger", new
        {
            regelId = regel.RegelId, type = "Deterministisk", utfall = "IkkeOppfylt", eskalert = true,
            forelderVurderingId = rot!.VurderingId, faktumIder = new[] { faktum!.FaktumId }
        });

        var forklaring = await HentSakForklaringAsync(client, sak.SakId);

        Assert.False(forklaring.Oppsummering.HarVedtak);
        Assert.Equal(0, forklaring.Oppsummering.AntallVedtak);
        Assert.Equal(2, forklaring.Oppsummering.AntallVurderinger);
        Assert.Equal(2, forklaring.Oppsummering.AntallEskalerteVurderinger);
        Assert.Empty(forklaring.Vedtak);

        var tre = Assert.Single(forklaring.Vurderingstre);
        Assert.Single(tre.Delvurderinger);
        Assert.All(forklaring.Vurderinger, v => Assert.False(v.ErLaast));
        Assert.All(forklaring.Faktum, f => Assert.False(f.ErLaast));
        Assert.Single(forklaring.Referansedata.Kilder);
        Assert.Single(forklaring.Referansedata.Regler);
    }

    [Fact]
    public async Task Ukjent_sak_eller_vedtak_gir_404()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/saker/{Guid.NewGuid()}/forklaring")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/vedtak/{Guid.NewGuid()}/forklaring")).StatusCode);
    }

    private record IdResult(Guid RettskildeId, Guid KildeId, Guid RegelId, Guid SakId, Guid FaktumId, Guid VurderingId);
    private record SakRef(Guid SakId, string Tittel);
    private record VedtakRef(Guid VedtakId);
    private record Vurd(Guid VurderingId, Guid SakId, Guid RegelId, Guid? ForelderVurderingId, bool ErLaast, List<Guid> RefererteVurderingIder);
    private record VurdNode(Guid VurderingId, Guid? ForelderVurderingId, List<VurdNode> Delvurderinger);
    private record Fakt(Guid FaktumId, Guid SakId, bool ErLaast);
    private record KildeR(string Navn);
    private record VilkarR(string? Kode);
    private record RegelR(Guid RegelId);
    private record Ref(List<KildeR> Kilder, List<object> Rettskilder, List<RegelR> Regler, List<VilkarR> Vilkar);
    private record VedtakForklaring(List<Vurd> Vurderinger, List<VurdNode> Vurderingstre, Ref Referansedata);
    private record Oppsummering(bool HarVedtak, int AntallVedtak, int AntallFaktum, int AntallVurderinger, int AntallEskalerteVurderinger);
    private record Logg(List<object> Oppforinger);
    private record VedtakMedVirkninger(Logg Forklaringslogg, List<object> Virkninger);
    private record SakR(Guid SakId);
    private record SakForklaring(
        SakR Sak, Oppsummering Oppsummering, List<Fakt> Faktum, List<Vurd> Vurderinger, List<VurdNode> Vurderingstre,
        List<VedtakMedVirkninger> Vedtak, List<Vurd> RefererteVurderinger, List<Fakt> AndreFaktum, Ref Referansedata);
}
