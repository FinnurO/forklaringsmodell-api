using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class RettskildeConfiguration : IEntityTypeConfiguration<Rettskilde>
{
    public void Configure(EntityTypeBuilder<Rettskilde> builder)
    {
        builder.ToTable("Rettskilder");
        builder.HasKey(x => x.RettskildeId);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Henvisning).IsRequired().HasMaxLength(300);
        builder.Property(x => x.EliReferanse).HasMaxLength(500);
    }
}
