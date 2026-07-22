using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Services;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Forklaringsmodell.Infrastructure.Repositories;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>
/// Regel 3.12: en Vilkar-rad som er referert av minst én Vedtaksvirkning skal ikke
/// overskrives (samme append-only-mønster som Regel, regel 3.4).
/// </summary>
public class VilkarAppendOnlyTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly VilkarService _vilkarService;
    private readonly Vilkar _referertVilkar;
    private readonly Vilkar _ureferertVilkar;

    public VilkarAppendOnlyTests()
    {
        _testDb = TestDbContextFactory.Create();
        var db = _testDb.Context;

        var sak = new Sak { SakId = Guid.NewGuid(), Tittel = "Test", Status = SakStatus.UnderBehandling, Opprettet = DateTimeOffset.UtcNow, SistEndret = DateTimeOffset.UtcNow, UtlosendeHendelse = HendelseType.Soknad };
        var vedtak = new Vedtak { VedtakId = Guid.NewGuid(), SakId = sak.SakId, Tidspunkt = DateTimeOffset.UtcNow, Utfall = "Test", AutomatiseringsGrad = AutomatiseringsGrad.Helautomatisert };

        _referertVilkar = new Vilkar { VilkarId = Guid.NewGuid(), Navn = "Referert vilkår", Type = VirkningType.Tillatelse, Fastsettelsesmate = FastsettelsesmateType.Statisk };
        _ureferertVilkar = new Vilkar { VilkarId = Guid.NewGuid(), Navn = "Ureferert vilkår", Type = VirkningType.Tillatelse, Fastsettelsesmate = FastsettelsesmateType.Statisk };

        var virkning = new Vedtaksvirkning
        {
            VirkningId = Guid.NewGuid(),
            VedtakId = vedtak.VedtakId,
            VilkarId = _referertVilkar.VilkarId,
            Type = VirkningType.Tillatelse,
            Fastsettelsesmate = FastsettelsesmateType.Statisk,
            Beskrivelse = "Test-virkning",
            Varighet = VarighetsType.Varig
        };

        db.Saker.Add(sak);
        db.Vedtak.Add(vedtak);
        db.Vilkar.AddRange(_referertVilkar, _ureferertVilkar);
        db.Vedtaksvirkninger.Add(virkning);
        db.SaveChanges();

        var repository = new ForklaringsmodellRepository(db);
        _vilkarService = new VilkarService(repository, new OpprettVilkarDtoValidator());
    }

    [Fact]
    public async Task Referert_vilkar_kan_ikke_endres()
    {
        await Assert.ThrowsAsync<AppendOnlyViolationException>(
            () => _vilkarService.SjekkKanEndreAsync(_referertVilkar.VilkarId));
    }

    [Fact]
    public async Task Ureferert_vilkar_kan_endres()
    {
        await _vilkarService.SjekkKanEndreAsync(_ureferertVilkar.VilkarId);
        // Ingen exception kastet - testen passerer implisitt.
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
