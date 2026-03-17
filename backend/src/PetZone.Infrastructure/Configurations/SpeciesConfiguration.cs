using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Domain.Species; // Обратите внимание на ваш namespace

namespace PetZone.Infrastructure.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.ToTable("Species");
        
        builder.HasKey(s => s.Id);
        
        // ИСПОЛЬЗУЕМ КОНСТАНТУ ИЗ СУЩНОСТИ (например, 50 или 100 символов)
        builder.Property(s => s.Name)
            .HasMaxLength(Species.MAX_NAME_LENGTH)
            .IsRequired();

        // --- НАСТРОЙКА СВЯЗИ ОДИН КО МНОГИМ ---
        builder.HasMany(s => s.Breeds)
            .WithOne() 
            .HasForeignKey("SpeciesId") 
            .OnDelete(DeleteBehavior.Cascade); 

        // Указываем EF Core читать/писать коллекцию через приватное поле _breeds
        builder.Navigation(s => s.Breeds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}