using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forklaringsmodell.Infrastructure;

public class ForklaringsmodellDbContext : DbContext
{
    public ForklaringsmodellDbContext(DbContextOptions<ForklaringsmodellDbContext> options) : base(options)
    {
    }

    public DbSet<Sak> Saker => Set<Sak>();
    public DbSet<SakRelasjon> SakRelasjoner => Set<SakRelasjon>();
    public DbSet<Kilde> Kilder => Set<Kilde>();
    public DbSet<Faktum> Faktum => Set<Faktum>();
    public DbSet<Rettskilde> Rettskilder => Set<Rettskilde>();
    public DbSet<Regel> Regler => Set<Regel>();
    public DbSet<RegelRettskilde> RegelRettskilde => Set<RegelRettskilde>();
    public DbSet<KildeRettskilde> KildeRettskilde => Set<KildeRettskilde>();
    public DbSet<FaktumRettskilde> FaktumRettskilde => Set<FaktumRettskilde>();
    public DbSet<VurderingRettskilde> VurderingRettskilde => Set<VurderingRettskilde>();
    public DbSet<Vurdering> Vurderinger => Set<Vurdering>();
    public DbSet<VurderingFaktum> VurderingFaktum => Set<VurderingFaktum>();
    public DbSet<VurderingReferanse> VurderingReferanse => Set<VurderingReferanse>();
    public DbSet<Partsmedvirkning> Partsmedvirkninger => Set<Partsmedvirkning>();
    public DbSet<Vedtak> Vedtak => Set<Vedtak>();
    public DbSet<Vedtaksvirkning> Vedtaksvirkninger => Set<Vedtaksvirkning>();
    public DbSet<VedtaksvirkningVurdering> VedtaksvirkningVurdering => Set<VedtaksvirkningVurdering>();
    public DbSet<VedtaksvirkningFaktum> VedtaksvirkningFaktum => Set<VedtaksvirkningFaktum>();
    public DbSet<Forklaringslogg> Forklaringslogger => Set<Forklaringslogg>();
    public DbSet<ForklaringsloggOppforing> ForklaringsloggOppforinger => Set<ForklaringsloggOppforing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ForklaringsmodellDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
