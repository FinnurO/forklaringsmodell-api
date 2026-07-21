using Forklaringsmodell.Application.Exceptions;
using Forklaringsmodell.Application.Services;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Forklaringsmodell.Infrastructure.Repositories;

namespace Forklaringsmodell.Tests.Unit;

/// <summary>
/// Regel 3.1: forsøk på å endre et Faktum som er referert av en ForklaringsloggOppforing
/// skal avvises med AppendOnlyViolationException (mappes til 409 Conflict i Api-laget,
/// se ExceptionHandlingMiddleware).
/// </summary>
public class FaktumAppendOnlyTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly FaktumService _faktumService;
    private readonly Faktum _referertFaktum;
    private readonly Faktum _ureferertFaktum;

    public FaktumAppendOnlyTests()
    {
        _testDb = TestDbContextFactory.Create();
        var db = _testDb.Context;

        var sak = new Sak { SakId = Guid.NewGuid(), Tittel = "Test", Status = SakStatus.UnderBehandling, Opprettet = DateTimeOffset.UtcNow, SistEndret = DateTimeOffset.UtcNow };
        var kilde = new Kilde { KildeId = Guid.NewGuid(), Navn = "Test-kilde", Type = KildeType.AnnenKilde, Autoritativ = false };

        _referertFaktum = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sak.SakId,
            KildeId = kilde.KildeId,
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Ustrukturert,
            Verdi = "Referert faktum",
            InnhentetTidspunkt = DateTimeOffset.UtcNow
        };

        _ureferertFaktum = new Faktum
        {
            FaktumId = Guid.NewGuid(),
            SakId = sak.SakId,
            KildeId = kilde.KildeId,
            Type = FaktumType.Raatt,
            Struktur = StrukturType.Ustrukturert,
            Verdi = "Ureferert faktum",
            InnhentetTidspunkt = DateTimeOffset.UtcNow
        };

        var vedtak = new Vedtak
        {
            VedtakId = Guid.NewGuid(),
            SakId = sak.SakId,
            Tidspunkt = DateTimeOffset.UtcNow,
            Utfall = "Test-utfall",
            AutomatiseringsGrad = AutomatiseringsGrad.Helautomatisert
        };

        var logg = new Forklaringslogg { LoggId = Guid.NewGuid(), VedtakId = vedtak.VedtakId };
        logg.Oppforinger.Add(new ForklaringsloggOppforing
        {
            OppforingId = Guid.NewGuid(),
            LoggId = logg.LoggId,
            Type = OppforingsType.Faktum,
            ReferanseId = _referertFaktum.FaktumId
        });

        db.Saker.Add(sak);
        db.Kilder.Add(kilde);
        db.Faktum.AddRange(_referertFaktum, _ureferertFaktum);
        db.Vedtak.Add(vedtak);
        db.Forklaringslogger.Add(logg);
        db.SaveChanges();

        var repository = new ForklaringsmodellRepository(db);
        _faktumService = new FaktumService(repository, new OpprettFaktumDtoValidator(), new TransformerFaktumDtoValidator());
    }

    [Fact]
    public async Task Referert_faktum_kan_ikke_endres()
    {
        await Assert.ThrowsAsync<AppendOnlyViolationException>(
            () => _faktumService.SjekkKanEndreAsync(_referertFaktum.FaktumId));
    }

    [Fact]
    public async Task Ureferert_faktum_kan_endres()
    {
        await _faktumService.SjekkKanEndreAsync(_ureferertFaktum.FaktumId);
        // Ingen exception kastet - testen passerer implisitt.
    }

    [Fact]
    public async Task Transformer_oppretter_nytt_subsumert_faktum_med_avledetfra_satt()
    {
        var nyttFaktum = await _faktumService.TransformerAsync(
            _referertFaktum.FaktumId,
            new Forklaringsmodell.Application.Dtos.TransformerFaktumDto { Verdi = "Subsumert verdi" });

        Assert.Equal(FaktumType.Subsumert, nyttFaktum.Type);
        Assert.Equal(_referertFaktum.FaktumId, nyttFaktum.AvledetFraFaktumId);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
