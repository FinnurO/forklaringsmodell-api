using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class FaktumConfiguration : IEntityTypeConfiguration<Faktum>
{
    public void Configure(EntityTypeBuilder<Faktum> builder)
    {
        builder.ToTable("Faktum");
        builder.HasKey(x => x.FaktumId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Struktur).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Verdi).IsRequired();
        builder.Property(x => x.InnhentetTidspunkt).IsRequired();

        // Beregnet, ikke lagret (se kommentar på Faktum.ErLaast).
        builder.Ignore(x => x.ErLaast);

        builder.HasIndex(x => x.SakId);
        builder.HasIndex(x => x.KildeId);

        builder.HasOne(x => x.AvledetFraFaktum)
            .WithMany(x => x.Avledninger)
            .HasForeignKey(x => x.AvledetFraFaktumId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
