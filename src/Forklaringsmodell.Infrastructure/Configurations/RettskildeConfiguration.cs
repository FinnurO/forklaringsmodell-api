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

        builder.Property(x => x.Paragraf).IsRequired().HasMaxLength(300);
        builder.Property(x => x.VersjonDato).IsRequired();
        builder.Property(x => x.EliReferanse).IsRequired().HasMaxLength(500);

        builder.HasMany(x => x.Regler).WithOne(r => r.Rettskilde).HasForeignKey(r => r.RettskildeId).OnDelete(DeleteBehavior.Restrict);
    }
}
