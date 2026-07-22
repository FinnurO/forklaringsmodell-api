using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Domain.Entities;
using Forklaringsmodell.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Forklaringsmodell.Infrastructure.Repositories;

public class ForklaringsmodellRepository : IForklaringsmodellRepository
{
    private readonly ForklaringsmodellDbContext _db;

    public ForklaringsmodellRepository(ForklaringsmodellDbContext db)
    {
        _db = db;
    }

    // Sak
    public Task<Sak?> GetSakAsync(Guid sakId, CancellationToken ct = default) =>
        _db.Saker.FirstOrDefaultAsync(s => s.SakId == sakId, ct);

    // SQLite kan ikke oversette ORDER BY på DateTimeOffset til SQL, så sorteringen skjer
    // klientsidig etter materialisering.
    public async Task<List<Sak>> GetSakerAsync(CancellationToken ct = default) =>
        (await _db.Saker.ToListAsync(ct)).OrderByDescending(s => s.Opprettet).ToList();

    public async Task AddSakAsync(Sak sak, CancellationToken ct = default) =>
        await _db.Saker.AddAsync(sak, ct);

    // SakRelasjon
    public Task<List<SakRelasjon>> GetSakRelasjonerForSakAsync(Guid sakId, CancellationToken ct = default) =>
        _db.SakRelasjoner.Where(r => r.SakId == sakId).ToListAsync(ct);

    public async Task AddSakRelasjonAsync(SakRelasjon relasjon, CancellationToken ct = default) =>
        await _db.SakRelasjoner.AddAsync(relasjon, ct);

    // Kilde
    public Task<Kilde?> GetKildeAsync(Guid kildeId, CancellationToken ct = default) =>
        _db.Kilder.Include(k => k.KildeRettskilde).FirstOrDefaultAsync(k => k.KildeId == kildeId, ct);

    public Task<List<Kilde>> GetKilderAsync(CancellationToken ct = default) =>
        _db.Kilder.Include(k => k.KildeRettskilde).OrderBy(k => k.Navn).ToListAsync(ct);

    public async Task AddKildeAsync(Kilde kilde, CancellationToken ct = default) =>
        await _db.Kilder.AddAsync(kilde, ct);

    public Task<bool> ErKildeReferertAsync(Guid kildeId, CancellationToken ct = default) =>
        _db.Faktum.AnyAsync(f => f.KildeId == kildeId, ct);

    // Faktum
    public Task<Faktum?> GetFaktumAsync(Guid faktumId, CancellationToken ct = default) =>
        _db.Faktum.Include(f => f.VurderingFaktum).Include(f => f.FaktumRettskilde).FirstOrDefaultAsync(f => f.FaktumId == faktumId, ct);

    // Se kommentar ved GetSakerAsync ang. klientsidig sortering på DateTimeOffset.
    public async Task<List<Faktum>> GetFaktumForSakAsync(Guid sakId, CancellationToken ct = default) =>
        (await _db.Faktum.Include(f => f.FaktumRettskilde).Where(f => f.SakId == sakId).ToListAsync(ct)).OrderBy(f => f.InnhentetTidspunkt).ToList();

    public Task<List<Faktum>> GetFaktumByIderAsync(IEnumerable<Guid> faktumIder, CancellationToken ct = default) =>
        _db.Faktum.Include(f => f.VurderingFaktum).Include(f => f.FaktumRettskilde).Where(f => faktumIder.Contains(f.FaktumId)).ToListAsync(ct);

    public async Task AddFaktumAsync(Faktum faktum, CancellationToken ct = default) =>
        await _db.Faktum.AddAsync(faktum, ct);

    public Task<bool> ErFaktumReferertAsync(Guid faktumId, CancellationToken ct = default) =>
        _db.ForklaringsloggOppforinger.AnyAsync(o => o.Type == OppforingsType.Faktum && o.ReferanseId == faktumId, ct);

    // Rettskilde
    public Task<Rettskilde?> GetRettskildeAsync(Guid rettskildeId, CancellationToken ct = default) =>
        _db.Rettskilder.FirstOrDefaultAsync(r => r.RettskildeId == rettskildeId, ct);

    public Task<List<Rettskilde>> GetRettskilderAsync(CancellationToken ct = default) =>
        _db.Rettskilder.OrderBy(r => r.Henvisning).ToListAsync(ct);

    public Task<List<Rettskilde>> GetRettskilderByIderAsync(IEnumerable<Guid> rettskildeIder, CancellationToken ct = default) =>
        _db.Rettskilder.Where(r => rettskildeIder.Contains(r.RettskildeId)).ToListAsync(ct);

    public async Task AddRettskildeAsync(Rettskilde rettskilde, CancellationToken ct = default) =>
        await _db.Rettskilder.AddAsync(rettskilde, ct);

    // Regel
    public Task<Regel?> GetRegelAsync(Guid regelId, CancellationToken ct = default) =>
        _db.Regler.Include(r => r.RegelRettskilde).FirstOrDefaultAsync(r => r.RegelId == regelId, ct);

    public Task<List<Regel>> GetReglerAsync(CancellationToken ct = default) =>
        _db.Regler.Include(r => r.RegelRettskilde).OrderBy(r => r.Teknologi).ToListAsync(ct);

    public async Task AddRegelAsync(Regel regel, CancellationToken ct = default) =>
        await _db.Regler.AddAsync(regel, ct);

    public Task<bool> ErRegelReferertAsync(Guid regelId, CancellationToken ct = default) =>
        _db.Vurderinger.AnyAsync(v => v.RegelId == regelId, ct);

    // Vurdering
    public Task<Vurdering?> GetVurderingAsync(Guid vurderingId, CancellationToken ct = default) =>
        _db.Vurderinger.Include(v => v.VurderingFaktum).Include(v => v.VurderingRettskilde).Include(v => v.RefererteVurderinger)
            .FirstOrDefaultAsync(v => v.VurderingId == vurderingId, ct);

    public Task<List<Vurdering>> GetVurderingerForSakAsync(Guid sakId, CancellationToken ct = default) =>
        _db.Vurderinger.Include(v => v.VurderingFaktum).Include(v => v.VurderingRettskilde).Include(v => v.RefererteVurderinger)
            .Where(v => v.SakId == sakId).ToListAsync(ct);

    public Task<List<Vurdering>> GetVurderingerByIderAsync(IEnumerable<Guid> vurderingIder, CancellationToken ct = default) =>
        _db.Vurderinger.Include(v => v.VurderingFaktum).Include(v => v.VurderingRettskilde).Include(v => v.RefererteVurderinger)
            .Where(v => vurderingIder.Contains(v.VurderingId)).ToListAsync(ct);

    public async Task AddVurderingAsync(Vurdering vurdering, CancellationToken ct = default) =>
        await _db.Vurderinger.AddAsync(vurdering, ct);

    public Task<bool> ErVurderingReferertAsync(Guid vurderingId, CancellationToken ct = default) =>
        _db.ForklaringsloggOppforinger.AnyAsync(o => o.Type == OppforingsType.Vurdering && o.ReferanseId == vurderingId, ct);

    // Partsmedvirkning
    public Task<Partsmedvirkning?> GetPartsmedvirkningAsync(Guid medvirkningId, CancellationToken ct = default) =>
        _db.Partsmedvirkninger.FirstOrDefaultAsync(p => p.MedvirkningId == medvirkningId, ct);

    // Se kommentar ved GetSakerAsync ang. klientsidig sortering på DateTimeOffset.
    public async Task<List<Partsmedvirkning>> GetPartsmedvirkningerForSakAsync(Guid sakId, CancellationToken ct = default) =>
        (await _db.Partsmedvirkninger.Where(p => p.SakId == sakId).ToListAsync(ct)).OrderBy(p => p.Tidspunkt).ToList();

    public Task<List<Partsmedvirkning>> GetPartsmedvirkningerByIderAsync(IEnumerable<Guid> medvirkningIder, CancellationToken ct = default) =>
        _db.Partsmedvirkninger.Where(p => medvirkningIder.Contains(p.MedvirkningId)).ToListAsync(ct);

    public async Task AddPartsmedvirkningAsync(Partsmedvirkning partsmedvirkning, CancellationToken ct = default) =>
        await _db.Partsmedvirkninger.AddAsync(partsmedvirkning, ct);

    // Vedtak / Forklaringslogg
    public Task<Vedtak?> GetVedtakAsync(Guid vedtakId, CancellationToken ct = default) =>
        _db.Vedtak.FirstOrDefaultAsync(v => v.VedtakId == vedtakId, ct);

    public Task<Forklaringslogg?> GetForklaringsloggAsync(Guid vedtakId, CancellationToken ct = default) =>
        _db.Forklaringslogger.Include(l => l.Oppforinger).FirstOrDefaultAsync(l => l.VedtakId == vedtakId, ct);

    public async Task AddVedtakMedForklaringsloggAsync(Vedtak vedtak, Forklaringslogg logg, CancellationToken ct = default)
    {
        await _db.Vedtak.AddAsync(vedtak, ct);
        await _db.Forklaringslogger.AddAsync(logg, ct);
    }

    // Vedtaksvirkning
    public Task<List<Vedtaksvirkning>> GetVirkningerForVedtakAsync(Guid vedtakId, CancellationToken ct = default) =>
        _db.Vedtaksvirkninger.Include(v => v.VedtaksvirkningVurdering).Include(v => v.VedtaksvirkningFaktum)
            .Where(v => v.VedtakId == vedtakId).ToListAsync(ct);

    public async Task AddVedtaksvirkningAsync(Vedtaksvirkning virkning, CancellationToken ct = default) =>
        await _db.Vedtaksvirkninger.AddAsync(virkning, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await _db.Database.BeginTransactionAsync(ct);
        return new TransactionWrapper(transaction);
    }

    /// <summary>
    /// Committer transaksjonen når den disposes. Kalleren (VedtakService) er ansvarlig
    /// for at SaveChangesAsync er kalt før wrapperen disposes, slik at commit skjer etter
    /// at alle rader (Vedtak, Forklaringslogg, ForklaringsloggOppforing) faktisk er skrevet.
    /// </summary>
    private sealed class TransactionWrapper : IAsyncDisposable
    {
        private readonly Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction _transaction;

        public TransactionWrapper(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
        }
    }
}
