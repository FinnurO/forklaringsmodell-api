using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class KildeConfiguration : IEntityTypeConfiguration<Kilde>
{
    public void Configure(EntityTypeBuilder<Kilde> builder)
    {
        builder.ToTable("Kilder");
        builder.HasKey(x => x.KildeId);

        builder.Property(x => x.Navn).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Autoritativ).IsRequired();
        builder.Property(x => x.CpsvReferanse).HasMaxLength(500);

        builder.Ignore(x => x.ErLaast);

        builder.HasMany(x => x.Faktum).WithOne(f => f.Kilde).HasForeignKey(f => f.KildeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class KildeRettskildeConfiguration : IEntityTypeConfiguration<KildeRettskilde>
{
    public void Configure(EntityTypeBuilder<KildeRettskilde> builder)
    {
        builder.ToTable("KildeRettskilde");
        builder.HasKey(x => new { x.KildeId, x.RettskildeId });

        builder.HasOne(x => x.Kilde)
            .WithMany(k => k.KildeRettskilde)
            .HasForeignKey(x => x.KildeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Rettskilde)
            .WithMany(r => r.KildeRettskilde)
            .HasForeignKey(x => x.RettskildeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
