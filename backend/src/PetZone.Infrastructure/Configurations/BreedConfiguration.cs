using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Domain.Species;

namespace PetZone.Infrastructure.Configurations;

public class BreedConfiguration : IEntityTypeConfiguration<Breed>
{
    public void Configure(EntityTypeBuilder<Breed> builder)
    {
        builder.ToTable("Breeds");
        
        builder.HasKey(b => b.Id);
        
        // ПРИМЕНЯЕМ НАШУ КОНСТАНТУ!
        builder.Property(b => b.Name)
            .HasMaxLength(Breed.MAX_NAME_LENGTH)
            .IsRequired();
    }
}
