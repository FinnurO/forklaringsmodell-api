using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class VurderingConfiguration : IEntityTypeConfiguration<Vurdering>
{
    public void Configure(EntityTypeBuilder<Vurdering> builder)
    {
        builder.ToTable("Vurderinger");
        builder.HasKey(x => x.VurderingId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Konfidens).HasPrecision(3, 2); // 0.00-1.00
        builder.Property(x => x.Eskalert).IsRequired();

        builder.Ignore(x => x.ErLaast);

        builder.HasIndex(x => x.SakId);
        builder.HasIndex(x => x.RegelId);
    }
}

public class VurderingFaktumConfiguration : IEntityTypeConfiguration<VurderingFaktum>
{
    public void Configure(EntityTypeBuilder<VurderingFaktum> builder)
    {
        builder.ToTable("VurderingFaktum");
        builder.HasKey(x => new { x.VurderingId, x.FaktumId });

        builder.HasOne(x => x.Vurdering)
            .WithMany(v => v.VurderingFaktum)
            .HasForeignKey(x => x.VurderingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Faktum)
            .WithMany(f => f.VurderingFaktum)
            .HasForeignKey(x => x.FaktumId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VurderingRettskildeConfiguration : IEntityTypeConfiguration<VurderingRettskilde>
{
    public void Configure(EntityTypeBuilder<VurderingRettskilde> builder)
    {
        builder.ToTable("VurderingRettskilde");
        builder.HasKey(x => new { x.VurderingId, x.RettskildeId });

        builder.HasOne(x => x.Vurdering)
            .WithMany(v => v.VurderingRettskilde)
            .HasForeignKey(x => x.VurderingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Rettskilde)
            .WithMany(r => r.VurderingRettskilde)
            .HasForeignKey(x => x.RettskildeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
