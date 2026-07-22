using System.Net;
using System.Net.Http.Json;

namespace Forklaringsmodell.Tests.Integration;

/// <summary>
/// Regel 3.1: ingen PUT/DELETE på /api/vedtak/{id} - siden kun GET er mappet til denne
/// ruten (se VedtakController), skal ASP.NET Core sin routing gi 405 Method Not Allowed
/// (ikke 404) for andre HTTP-verb mot samme rute.
/// </summary>
public class VedtakEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VedtakEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Put_mot_vedtak_gir_405()
    {
        var client = _factory.CreateClient();
        var vedtakId = Guid.NewGuid();

        var response = await client.PutAsJsonAsync($"/api/vedtak/{vedtakId}", new { utfall = "Endret" });

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Delete_mot_vedtak_gir_405()
    {
        var client = _factory.CreateClient();
        var vedtakId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/vedtak/{vedtakId}");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Put_mot_vedtak_forklaring_gir_405()
    {
        var client = _factory.CreateClient();
        var vedtakId = Guid.NewGuid();

        var response = await client.PutAsJsonAsync($"/api/vedtak/{vedtakId}/forklaring", new { });

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Full_flyt_opprett_sak_faktum_vurdering_vedtak_og_les_forklaring()
    {
        var client = _factory.CreateClient();

        var rettskildeResponse = await client.PostAsJsonAsync("/api/rettskilder", new
        {
            type = "Lov",
            henvisning = "Test-henvisning",
            versjonDato = DateTimeOffset.UtcNow,
            eliReferanse = "https://example.test/test"
        });
        rettskildeResponse.EnsureSuccessStatusCode();
        var rettskilde = await rettskildeResponse.Content.ReadFromJsonAsync<RettskildeResult>();

        var kildeResponse = await client.PostAsJsonAsync("/api/kilder", new
        {
            navn = "Test-kilde",
            type = "AnnenKilde",
            autoritativ = false,
            rettskildeIder = new[] { rettskilde!.RettskildeId }
        });
        kildeResponse.EnsureSuccessStatusCode();
        var kilde = await kildeResponse.Content.ReadFromJsonAsync<KildeResult>();

        var sakResponse = await client.PostAsJsonAsync("/api/saker", new { tittel = "Integrasjonstest-sak", status = "UnderBehandling", utlosendeHendelse = "Soknad" });
        sakResponse.EnsureSuccessStatusCode();
        var sak = await sakResponse.Content.ReadFromJsonAsync<SakResult>();

        var faktumResponse = await client.PostAsJsonAsync($"/api/saker/{sak!.SakId}/faktum", new
        {
            kildeId = kilde!.KildeId,
            type = "Raatt",
            struktur = "Ustrukturert",
            verdi = "Test faktum"
        });
        faktumResponse.EnsureSuccessStatusCode();
        var faktum = await faktumResponse.Content.ReadFromJsonAsync<FaktumResult>();

        var regelResponse = await client.PostAsJsonAsync("/api/regler", new
        {
            rettskildeIder = new[] { rettskilde.RettskildeId },
            teknologi = "Test",
            type = "Deterministisk"
        });
        regelResponse.EnsureSuccessStatusCode();
        var regel = await regelResponse.Content.ReadFromJsonAsync<RegelResult>();

        var vurderingResponse = await client.PostAsJsonAsync($"/api/saker/{sak.SakId}/vurderinger", new
        {
            regelId = regel!.RegelId,
            type = "Deterministisk",
            beregningsspor = "test",
            eskalert = false,
            faktumIder = new[] { faktum!.FaktumId }
        });
        vurderingResponse.EnsureSuccessStatusCode();
        var vurdering = await vurderingResponse.Content.ReadFromJsonAsync<VurderingResult>();

        var vedtakResponse = await client.PostAsJsonAsync($"/api/saker/{sak.SakId}/vedtak", new
        {
            utfall = "Test-utfall",
            faktumIder = new[] { faktum.FaktumId },
            vurderingIder = new[] { vurdering!.VurderingId },
            partsmedvirkningIder = Array.Empty<Guid>(),
            virkninger = new[]
            {
                new
                {
                    type = "OkonomiskYtelse",
                    beskrivelse = "Test-ytelse",
                    varighet = "Tidsbegrenset",
                    gyldigFra = DateTimeOffset.UtcNow,
                    gyldigTil = DateTimeOffset.UtcNow.AddMonths(6),
                    belop = 1000,
                    vurderingIder = new[] { vurdering.VurderingId },
                    faktumIder = new[] { faktum.FaktumId }
                }
            }
        });
        vedtakResponse.EnsureSuccessStatusCode();
        var vedtak = await vedtakResponse.Content.ReadFromJsonAsync<VedtakResult>();

        Assert.Equal("Helautomatisert", vedtak!.AutomatiseringsGrad);

        var forklaringResponse = await client.GetAsync($"/api/vedtak/{vedtak.VedtakId}/forklaring");
        forklaringResponse.EnsureSuccessStatusCode();
        var body = await forklaringResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test faktum", body);
        Assert.Contains("Test-ytelse", body);

        var virkningerResponse = await client.GetAsync($"/api/vedtak/{vedtak.VedtakId}/virkninger");
        virkningerResponse.EnsureSuccessStatusCode();
        var virkninger = await virkningerResponse.Content.ReadFromJsonAsync<List<VirkningResult>>();
        Assert.Single(virkninger!);
        Assert.Equal("Test-ytelse", virkninger![0].Beskrivelse);
    }

    private record KildeResult(Guid KildeId);
    private record SakResult(Guid SakId);
    private record FaktumResult(Guid FaktumId);
    private record RettskildeResult(Guid RettskildeId);
    private record RegelResult(Guid RegelId);
    private record VurderingResult(Guid VurderingId);
    private record VedtakResult(Guid VedtakId, string AutomatiseringsGrad);
    private record VirkningResult(Guid VirkningId, string Beskrivelse);
}
