using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpeciesEntity = PetZone.Species.Domain.Species;

namespace PetZone.Species.Infrastructure.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<SpeciesEntity>
{
    public void Configure(EntityTypeBuilder<SpeciesEntity> builder)
    {
        builder.ToTable("Species");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(SpeciesEntity.MAX_NAME_LENGTH)
            .IsRequired();

        builder.HasMany(s => s.Breeds)
            .WithOne()
            .HasForeignKey("SpeciesId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Breeds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}