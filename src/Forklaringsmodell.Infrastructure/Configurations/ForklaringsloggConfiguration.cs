using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class ForklaringsloggConfiguration : IEntityTypeConfiguration<Forklaringslogg>
{
    public void Configure(EntityTypeBuilder<Forklaringslogg> builder)
    {
        builder.ToTable("Forklaringslogger");
        builder.HasKey(x => x.LoggId);

        builder.HasMany(x => x.Oppforinger)
            .WithOne(o => o.Forklaringslogg)
            .HasForeignKey(o => o.LoggId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ForklaringsloggOppforingConfiguration : IEntityTypeConfiguration<ForklaringsloggOppforing>
{
    public void Configure(EntityTypeBuilder<ForklaringsloggOppforing> builder)
    {
        builder.ToTable("ForklaringsloggOppforinger");
        builder.HasKey(x => x.OppforingId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ReferanseId).IsRequired();

        builder.HasIndex(x => x.LoggId);
        builder.HasIndex(x => new { x.Type, x.ReferanseId });
    }
}
