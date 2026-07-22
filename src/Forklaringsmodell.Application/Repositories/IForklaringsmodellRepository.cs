using Forklaringsmodell.Domain.Entities;

namespace Forklaringsmodell.Application.Repositories;

/// <summary>
/// Pragmatisk valg: ett samlet repository-interface for hele modellen i stedet for ti
/// separate per-entitet-interfaces. Domenet er tett sammenknyttet (Vedtak-frysing
/// berører Faktum/Vurdering/Partsmedvirkning/Regel samtidig i én transaksjon), så en
/// unit-of-work-aktig fasade er enklere å resonnere om enn mange små repositories.
/// Implementeres av Forklaringsmodell.Infrastructure.Repositories.ForklaringsmodellRepository.
/// </summary>
public interface IForklaringsmodellRepository
{
    // Sak
    Task<Sak?> GetSakAsync(Guid sakId, CancellationToken ct = default);
    Task<List<Sak>> GetSakerAsync(CancellationToken ct = default);
    Task AddSakAsync(Sak sak, CancellationToken ct = default);

    // SakRelasjon
    Task<List<SakRelasjon>> GetSakRelasjonerForSakAsync(Guid sakId, CancellationToken ct = default);
    Task AddSakRelasjonAsync(SakRelasjon relasjon, CancellationToken ct = default);

    // Kilde
    Task<Kilde?> GetKildeAsync(Guid kildeId, CancellationToken ct = default);
    Task<List<Kilde>> GetKilderAsync(CancellationToken ct = default);
    Task AddKildeAsync(Kilde kilde, CancellationToken ct = default);
    Task<bool> ErKildeReferertAsync(Guid kildeId, CancellationToken ct = default);

    // Faktum
    Task<Faktum?> GetFaktumAsync(Guid faktumId, CancellationToken ct = default);
    Task<List<Faktum>> GetFaktumForSakAsync(Guid sakId, CancellationToken ct = default);
    Task<List<Faktum>> GetFaktumByIderAsync(IEnumerable<Guid> faktumIder, CancellationToken ct = default);
    Task AddFaktumAsync(Faktum faktum, CancellationToken ct = default);
    Task<bool> ErFaktumReferertAsync(Guid faktumId, CancellationToken ct = default);

    // Rettskilde
    Task<Rettskilde?> GetRettskildeAsync(Guid rettskildeId, CancellationToken ct = default);
    Task<List<Rettskilde>> GetRettskilderAsync(CancellationToken ct = default);
    Task<List<Rettskilde>> GetRettskilderByIderAsync(IEnumerable<Guid> rettskildeIder, CancellationToken ct = default);
    Task AddRettskildeAsync(Rettskilde rettskilde, CancellationToken ct = default);

    // Regel
    Task<Regel?> GetRegelAsync(Guid regelId, CancellationToken ct = default);
    Task<List<Regel>> GetReglerAsync(CancellationToken ct = default);
    Task AddRegelAsync(Regel regel, CancellationToken ct = default);
    Task<bool> ErRegelReferertAsync(Guid regelId, CancellationToken ct = default);

    // Vurdering
    Task<Vurdering?> GetVurderingAsync(Guid vurderingId, CancellationToken ct = default);
    Task<List<Vurdering>> GetVurderingerForSakAsync(Guid sakId, CancellationToken ct = default);
    Task<List<Vurdering>> GetVurderingerByIderAsync(IEnumerable<Guid> vurderingIder, CancellationToken ct = default);
    Task AddVurderingAsync(Vurdering vurdering, CancellationToken ct = default);
    Task<bool> ErVurderingReferertAsync(Guid vurderingId, CancellationToken ct = default);

    // Partsmedvirkning
    Task<Partsmedvirkning?> GetPartsmedvirkningAsync(Guid medvirkningId, CancellationToken ct = default);
    Task<List<Partsmedvirkning>> GetPartsmedvirkningerForSakAsync(Guid sakId, CancellationToken ct = default);
    Task<List<Partsmedvirkning>> GetPartsmedvirkningerByIderAsync(IEnumerable<Guid> medvirkningIder, CancellationToken ct = default);
    Task AddPartsmedvirkningAsync(Partsmedvirkning partsmedvirkning, CancellationToken ct = default);

    // Vedtak / Forklaringslogg
    Task<Vedtak?> GetVedtakAsync(Guid vedtakId, CancellationToken ct = default);
    Task<Forklaringslogg?> GetForklaringsloggAsync(Guid vedtakId, CancellationToken ct = default);
    Task AddVedtakMedForklaringsloggAsync(Vedtak vedtak, Forklaringslogg logg, CancellationToken ct = default);

    // Vedtaksvirkning
    Task<List<Vedtaksvirkning>> GetVirkningerForVedtakAsync(Guid vedtakId, CancellationToken ct = default);
    Task AddVedtaksvirkningAsync(Vedtaksvirkning virkning, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct = default);
}
