using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class FlerspraakligTekstConfiguration : IEntityTypeConfiguration<FlerspraakligTekst>
{
    public void Configure(EntityTypeBuilder<FlerspraakligTekst> builder)
    {
        builder.ToTable("FlerspraakligeTekster");
        builder.HasKey(x => x.FlerspraakligTekstId);
    }
}

public class TekstVariantConfiguration : IEntityTypeConfiguration<TekstVariant>
{
    public void Configure(EntityTypeBuilder<TekstVariant> builder)
    {
        builder.ToTable("TekstVarianter");
        builder.HasKey(x => new { x.FlerspraakligTekstId, x.SpraakKode });

        builder.Property(x => x.SpraakKode).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Verdi).IsRequired().HasMaxLength(4000);

        builder.HasOne(x => x.FlerspraakligTekst)
            .WithMany(t => t.Varianter)
            .HasForeignKey(x => x.FlerspraakligTekstId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
