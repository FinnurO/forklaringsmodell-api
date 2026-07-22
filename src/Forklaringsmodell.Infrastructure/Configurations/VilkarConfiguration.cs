using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class VilkarConfiguration : IEntityTypeConfiguration<Vilkar>
{
    public void Configure(EntityTypeBuilder<Vilkar> builder)
    {
        builder.ToTable("Vilkar");
        builder.HasKey(x => x.VilkarId);

        builder.Property(x => x.Navn).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Fastsettelsesmate).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.StandardTekst).HasMaxLength(2000);

        builder.HasOne(x => x.Regel)
            .WithMany()
            .HasForeignKey(x => x.RegelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VilkarRettskildeConfiguration : IEntityTypeConfiguration<VilkarRettskilde>
{
    public void Configure(EntityTypeBuilder<VilkarRettskilde> builder)
    {
        builder.ToTable("VilkarRettskilde");
        builder.HasKey(x => new { x.VilkarId, x.RettskildeId });

        builder.HasOne(x => x.Vilkar)
            .WithMany(v => v.VilkarRettskilde)
            .HasForeignKey(x => x.VilkarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Rettskilde)
            .WithMany(r => r.VilkarRettskilde)
            .HasForeignKey(x => x.RettskildeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
