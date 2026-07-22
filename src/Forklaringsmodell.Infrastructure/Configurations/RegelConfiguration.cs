using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class RegelConfiguration : IEntityTypeConfiguration<Regel>
{
    public void Configure(EntityTypeBuilder<Regel> builder)
    {
        builder.ToTable("Regler");
        builder.HasKey(x => x.RegelId);

        builder.Property(x => x.Teknologi).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CpsvRegelReferanse).HasMaxLength(500);

        builder.Ignore(x => x.ErLaast);

        builder.HasMany(x => x.Vurderinger).WithOne(v => v.Regel).HasForeignKey(v => v.RegelId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RegelRettskildeConfiguration : IEntityTypeConfiguration<RegelRettskilde>
{
    public void Configure(EntityTypeBuilder<RegelRettskilde> builder)
    {
        builder.ToTable("RegelRettskilde");
        builder.HasKey(x => new { x.RegelId, x.RettskildeId });

        builder.HasOne(x => x.Regel)
            .WithMany(r => r.RegelRettskilde)
            .HasForeignKey(x => x.RegelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Rettskilde)
            .WithMany(r => r.RegelRettskilde)
            .HasForeignKey(x => x.RettskildeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
