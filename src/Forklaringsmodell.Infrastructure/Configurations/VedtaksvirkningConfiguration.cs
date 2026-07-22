using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class VedtaksvirkningConfiguration : IEntityTypeConfiguration<Vedtaksvirkning>
{
    public void Configure(EntityTypeBuilder<Vedtaksvirkning> builder)
    {
        builder.ToTable("Vedtaksvirkninger");
        builder.HasKey(x => x.VirkningId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Beskrivelse).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Varighet).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Belop).HasPrecision(18, 2);
        builder.Property(x => x.LopendeVilkar).HasMaxLength(500);
        builder.Property(x => x.RapporteringsFrekvens).HasMaxLength(100);

        builder.HasOne(x => x.Vedtak)
            .WithMany(v => v.Virkninger)
            .HasForeignKey(x => x.VedtakId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.VedtakId);
    }
}

public class VedtaksvirkningVurderingConfiguration : IEntityTypeConfiguration<VedtaksvirkningVurdering>
{
    public void Configure(EntityTypeBuilder<VedtaksvirkningVurdering> builder)
    {
        builder.ToTable("VedtaksvirkningVurdering");
        builder.HasKey(x => new { x.VirkningId, x.VurderingId });

        builder.HasOne(x => x.Vedtaksvirkning)
            .WithMany(v => v.VedtaksvirkningVurdering)
            .HasForeignKey(x => x.VirkningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vurdering)
            .WithMany()
            .HasForeignKey(x => x.VurderingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VedtaksvirkningFaktumConfiguration : IEntityTypeConfiguration<VedtaksvirkningFaktum>
{
    public void Configure(EntityTypeBuilder<VedtaksvirkningFaktum> builder)
    {
        builder.ToTable("VedtaksvirkningFaktum");
        builder.HasKey(x => new { x.VirkningId, x.FaktumId });

        builder.HasOne(x => x.Vedtaksvirkning)
            .WithMany(v => v.VedtaksvirkningFaktum)
            .HasForeignKey(x => x.VirkningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Faktum)
            .WithMany()
            .HasForeignKey(x => x.FaktumId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
