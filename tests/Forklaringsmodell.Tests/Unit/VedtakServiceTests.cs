using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Forklaringsmodell.Infrastructure.Repositories;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>
/// Regel 3.5: Vedtak.AutomatiseringsGrad beregnes serverside ut fra andelen Vurdering med
/// Type == Skjonn og Eskalert == true, uavhengig av hva klienten evt. måtte sende inn
/// (OpprettVedtakDto har ikke et AutomatiseringsGrad-felt i det hele tatt, jf. spec punkt 5).
/// </summary>
public class VedtakServiceTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly VedtakService _vedtakService;
    private readonly Sak _sak;
    private readonly Regel _regel;

    public VedtakServiceTests()
    {
        _testDb = TestDbContextFactory.Create();
        var db = _testDb.Context;

        _sak = new Sak { SakId = Guid.NewGuid(), Tittel = "Test", Status = SakStatus.UnderBehandling, Opprettet = DateTimeOffset.UtcNow, SistEndret = DateTimeOffset.UtcNow };
        var rettskilde = new Rettskilde { RettskildeId = Guid.NewGuid(), Type = RettskildeType.Lov, Henvisning = "Test", VersjonDato = DateTimeOffset.UtcNow, EliReferanse = "test" };
        _regel = new Regel { RegelId = Guid.NewGuid(), Teknologi = "Test", Type = VurderingsType.Deterministisk };
        _regel.RegelRettskilde.Add(new RegelRettskilde { RegelId = _regel.RegelId, RettskildeId = rettskilde.RettskildeId });

        db.Saker.Add(_sak);
        db.Rettskilder.Add(rettskilde);
        db.Regler.Add(_regel);
        db.SaveChanges();

        var repository = new ForklaringsmodellRepository(db);
        _vedtakService = new VedtakService(repository, new OpprettVedtakDtoValidator());
        _repository = repository;
        _db = db;
    }

    private readonly Forklaringsmodell.Application.Repositories.IForklaringsmodellRepository _repository;
    private readonly Forklaringsmodell.Infrastructure.ForklaringsmodellDbContext _db;

    private Vurdering LeggTilVurdering(VurderingsType type, bool eskalert)
    {
        var vurdering = new Vurdering
        {
            VurderingId = Guid.NewGuid(),
            SakId = _sak.SakId,
            RegelId = _regel.RegelId,
            Type = type,
            Eskalert = eskalert,
            Hovedhensyn = type == VurderingsType.Skjonn ? "Test hovedhensyn" : null
        };
        _db.Vurderinger.Add(vurdering);
        _db.SaveChanges();
        return vurdering;
    }

    [Fact]
    public async Task Ingen_vurderinger_gir_manuell()
    {
        var vedtak = await _vedtakService.OpprettAsync(_sak.SakId, new OpprettVedtakDto { Utfall = "Test" });
        Assert.Equal(AutomatiseringsGrad.Manuell, vedtak.AutomatiseringsGrad);
    }

    [Fact]
    public async Task Kun_deterministiske_ikke_eskalerte_vurderinger_gir_helautomatisert()
    {
        var v1 = LeggTilVurdering(VurderingsType.Deterministisk, false);
        var v2 = LeggTilVurdering(VurderingsType.GenerativKI, false);

        var vedtak = await _vedtakService.OpprettAsync(_sak.SakId, new OpprettVedtakDto
        {
            Utfall = "Test",
            VurderingIder = new List<Guid> { v1.VurderingId, v2.VurderingId }
        });

        Assert.Equal(AutomatiseringsGrad.Helautomatisert, vedtak.AutomatiseringsGrad);
    }

    [Fact]
    public async Task Blanding_av_automatisert_og_skjonn_gir_delvis_automatisert()
    {
        var v1 = LeggTilVurdering(VurderingsType.Deterministisk, false);
        var v2 = LeggTilVurdering(VurderingsType.Skjonn, false);

        var vedtak = await _vedtakService.OpprettAsync(_sak.SakId, new OpprettVedtakDto
        {
            Utfall = "Test",
            VurderingIder = new List<Guid> { v1.VurderingId, v2.VurderingId }
        });

        Assert.Equal(AutomatiseringsGrad.DelvisAutomatisert, vedtak.AutomatiseringsGrad);
    }

    [Fact]
    public async Task Alle_skjonn_eller_eskalert_gir_manuell()
    {
        var v1 = LeggTilVurdering(VurderingsType.Skjonn, false);
        var v2 = LeggTilVurdering(VurderingsType.GenerativKI, true);

        var vedtak = await _vedtakService.OpprettAsync(_sak.SakId, new OpprettVedtakDto
        {
            Utfall = "Test",
            VurderingIder = new List<Guid> { v1.VurderingId, v2.VurderingId }
        });

        Assert.Equal(AutomatiseringsGrad.Manuell, vedtak.AutomatiseringsGrad);
    }

    [Fact]
    public async Task Faktum_og_vurdering_referert_i_vedtak_blir_laast()
    {
        var faktum = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = _sak.SakId,
            KildeId = Guid.NewGuid(),
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Ustrukturert,
            Verdi = "Test",
            InnhentetTidspunkt = DateTimeOffset.UtcNow
        };
        var kilde = new Kilde { KildeId = faktum.KildeId, Navn = "Test", Type = KildeType.AnnenKilde, Autoritativ = false };
        _db.Kilder.Add(kilde);
        _db.Faktum.Add(faktum);
        _db.SaveChanges();

        await _vedtakService.OpprettAsync(_sak.SakId, new OpprettVedtakDto
        {
            Utfall = "Test",
            FaktumIder = new List<Guid> { faktum.FaktumId }
        });

        var erReferert = await _repository.ErFaktumReferertAsync(faktum.FaktumId);
        Assert.True(erReferert);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
