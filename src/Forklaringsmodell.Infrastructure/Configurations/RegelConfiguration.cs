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

        builder.Ignore(x => x.ErLaast);

        builder.HasIndex(x => x.RettskildeId);

        builder.HasMany(x => x.Vurderinger).WithOne(v => v.Regel).HasForeignKey(v => v.RegelId).OnDelete(DeleteBehavior.Restrict);
    }
}
