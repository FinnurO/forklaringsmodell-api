using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class VedtakConfiguration : IEntityTypeConfiguration<Vedtak>
{
    public void Configure(EntityTypeBuilder<Vedtak> builder)
    {
        builder.ToTable("Vedtak");
        builder.HasKey(x => x.VedtakId);

        builder.Property(x => x.Tidspunkt).IsRequired();
        builder.Property(x => x.Utfall).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.AutomatiseringsGrad).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => x.SakId);

        builder.HasOne(x => x.Forklaringslogg)
            .WithOne(l => l.Vedtak)
            .HasForeignKey<Forklaringslogg>(l => l.VedtakId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
