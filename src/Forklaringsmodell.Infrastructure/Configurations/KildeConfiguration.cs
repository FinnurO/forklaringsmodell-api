using Forklaringsmodell.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forklaringsmodell.Infrastructure.Configurations;

public class KildeConfiguration : IEntityTypeConfiguration<Kilde>
{
    public void Configure(EntityTypeBuilder<Kilde> builder)
    {
        builder.ToTable("Kilder");
        builder.HasKey(x => x.KildeId);

        builder.Property(x => x.Navn).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Autoritativ).IsRequired();

        builder.HasMany(x => x.Faktum).WithOne(f => f.Kilde).HasForeignKey(f => f.KildeId).OnDelete(DeleteBehavior.Restrict);
    }
}
