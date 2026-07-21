using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class PartsmedvirkningConfiguration : IEntityTypeConfiguration<Partsmedvirkning>
{
    public void Configure(EntityTypeBuilder<Partsmedvirkning> builder)
    {
        builder.ToTable("Partsmedvirkninger");
        builder.HasKey(x => x.MedvirkningId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Tidspunkt).IsRequired();
        builder.Property(x => x.Innhold).IsRequired();

        builder.Ignore(x => x.ErLaast);

        builder.HasIndex(x => x.SakId);
    }
}
