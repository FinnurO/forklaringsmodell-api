using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class SakConfiguration : IEntityTypeConfiguration<Sak>
{
    public void Configure(EntityTypeBuilder<Sak> builder)
    {
        builder.ToTable("Saker");
        builder.HasKey(x => x.SakId);

        builder.Property(x => x.Tittel).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Opprettet).IsRequired();
        builder.Property(x => x.SistEndret).IsRequired();

        builder.HasMany(x => x.Faktum).WithOne(f => f.Sak).HasForeignKey(f => f.SakId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Partsmedvirkninger).WithOne(p => p.Sak).HasForeignKey(p => p.SakId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Vurderinger).WithOne(v => v.Sak).HasForeignKey(v => v.SakId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Vedtak).WithOne(v => v.Sak).HasForeignKey(v => v.SakId).OnDelete(DeleteBehavior.Restrict);
    }
}
