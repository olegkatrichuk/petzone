using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Domain.Species;

namespace PetZone.Infrastructure.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.ToTable("Species");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name).IsRequired();

        // --- НАСТРОЙКА СВЯЗИ ОДИН КО МНОГИМ ---
        builder.HasMany(s => s.Breeds)
            .WithOne() // У породы нет навигационного свойства обратно к виду (и это правильно для DDD)
            .HasForeignKey("SpeciesId") // Создаем теневой внешний ключ в таблице Breeds
            .OnDelete(DeleteBehavior.Cascade); // Если удаляем вид (например, "Собаки"), удаляются и все породы собак

        // Указываем EF Core читать/писать коллекцию через приватное поле _breeds
        builder.Navigation(s => s.Breeds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}