using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Options;
using Forklaringsmodell.Application.Services;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Forklaringsmodell.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>Regel 3.18: Vurdering.VilkarId og Vurdering.ForelderVurderingId — intra-sak tre, ingen håndhevet mal.</summary>
public class VurderingHierarkiTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly VurderingService _vurderingService;
    private readonly Sak _sak;
    private readonly Sak _annenSak;
    private readonly Regel _regel;
    private readonly Vilkar _vilkar;

    public VurderingHierarkiTests()
    {
        _testDb = TestDbContextFactory.Create();
        var db = _testDb.Context;

        _sak = new Sak { SakId = Guid.NewGuid(), Tittel = "Test", Status = SakStatus.UnderBehandling, Opprettet = DateTimeOffset.UtcNow, SistEndret = DateTimeOffset.UtcNow };
        _annenSak = new Sak { SakId = Guid.NewGuid(), Tittel = "Annen sak", Status = SakStatus.UnderBehandling, Opprettet = DateTimeOffset.UtcNow, SistEndret = DateTimeOffset.UtcNow };
        var rettskilde = new Rettskilde { RettskildeId = Guid.NewGuid(), Type = RettskildeType.Lov, Henvisning = "Test" };
        _regel = new Regel { RegelId = Guid.NewGuid(), Teknologi = "Test", Type = VurderingsType.Deterministisk };
        _regel.RegelRettskilde.Add(new RegelRettskilde { RegelId = _regel.RegelId, RettskildeId = rettskilde.RettskildeId });
        _vilkar = new Vilkar { VilkarId = Guid.NewGuid(), Navn = "Test-vilkår", Type = VirkningType.Tillatelse, Grunnlagstype = GrunnlagsType.InternPraksis, Fastsettelsesmate = FastsettelsesmateType.Statisk };

        db.Saker.AddRange(_sak, _annenSak);
        db.Rettskilder.Add(rettskilde);
        db.Regler.Add(_regel);
        db.Vilkar.Add(_vilkar);
        db.SaveChanges();

        var repository = new ForklaringsmodellRepository(db);
        _vurderingService = new VurderingService(repository, new OpprettVurderingDtoValidator(), Options.Create(new KonfidensTerskelOptions()));
    }

    [Fact]
    public async Task Vurdering_med_vilkarid_lagres_og_mappes()
    {
        var vurdering = await _vurderingService.OpprettAsync(_sak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt,
            VilkarId = _vilkar.VilkarId
        });

        Assert.Equal(_vilkar.VilkarId, vurdering.VilkarId);
    }

    [Fact]
    public async Task Ukjent_vilkarid_gir_notfound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _vurderingService.OpprettAsync(_sak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt,
            VilkarId = Guid.NewGuid()
        }));
    }

    [Fact]
    public async Task Delvurdering_i_samme_sak_lenkes_til_forelder_og_vises_i_delvurderingider()
    {
        var forelder = await _vurderingService.OpprettAsync(_sak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt
        });

        var barn = await _vurderingService.OpprettAsync(_sak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Uaktuelt,
            ForelderVurderingId = forelder.VurderingId
        });

        Assert.Equal(forelder.VurderingId, barn.ForelderVurderingId);

        var forelderEtterpa = await _vurderingService.GetAsync(forelder.VurderingId);
        Assert.Contains(barn.VurderingId, forelderEtterpa.DelvurderingIder);
    }

    [Fact]
    public async Task ForelderVurdering_i_annen_sak_gir_notfound()
    {
        var forelderIAnnenSak = await _vurderingService.OpprettAsync(_annenSak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt
        });

        await Assert.ThrowsAsync<NotFoundException>(() => _vurderingService.OpprettAsync(_sak.SakId, new OpprettVurderingDto
        {
            RegelId = _regel.RegelId,
            Type = VurderingsType.Deterministisk,
            Utfall = UtfallType.Oppfylt,
            ForelderVurderingId = forelderIAnnenSak.VurderingId
        }));
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
