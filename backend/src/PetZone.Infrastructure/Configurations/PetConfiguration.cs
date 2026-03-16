using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Domain.Models;

namespace PetZone.Infrastructure.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");
        
        builder.HasKey(p => p.Id);

        // --- ОБЫЧНЫЕ СВОЙСТВА ---
        builder.Property(p => p.Nickname).IsRequired();
        builder.Property(p => p.GeneralDescription).IsRequired();
        builder.Property(p => p.Color).IsRequired();
        builder.Property(p => p.IsCastrated).IsRequired();
        builder.Property(p => p.DateOfBirth).IsRequired();
        builder.Property(p => p.IsVaccinated).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        
        builder.Property(p => p.AdoptionConditions).IsRequired(false);
        builder.Property(p => p.MicrochipNumber).IsRequired(false);
        
        builder.Property(p => p.VolunteerId).IsRequired(false);

        // --- ОДИНОЧНЫЕ VALUE OBJECTS (Переводим на ComplexProperty!) ---
        
        builder.ComplexProperty(p => p.SpeciesBreedInfo, sb =>
        {
            // Теперь строго типизировано!
            sb.Property(p => p.SpeciesId).HasColumnName("species_id").IsRequired();
            sb.Property(p => p.BreedId).HasColumnName("breed_id").IsRequired();
        });

        builder.ComplexProperty(p => p.Health, h =>
        {
            h.Property(p => p.GeneralDescription).HasColumnName("health_general_description").IsRequired();
            h.Property(p => p.DietOrAllergies).HasColumnName("health_diet_or_allergies").IsRequired(false);
        });

        builder.ComplexProperty(p => p.Location, l =>
        {
            l.Property(p => p.City).HasColumnName("city").IsRequired();
            l.Property(p => p.Street).HasColumnName("street").IsRequired();
        });

        builder.ComplexProperty(p => p.Weight, w =>
        {
            w.Property(p => p.Value).HasColumnName("weight").IsRequired();
        });

        builder.ComplexProperty(p => p.Height, h =>
        {
            h.Property(p => p.Value).HasColumnName("height").IsRequired();
        });

        builder.ComplexProperty(p => p.OwnerPhone, ph =>
        {
            ph.Property(p => p.Value).HasColumnName("owner_phone").IsRequired();
        });

        // --- КОЛЛЕКЦИИ VALUE OBJECTS (JSON СТОЛБЦЫ) ---
        
        builder.OwnsMany(p => p.Requisites, r =>
        {
            r.ToJson(); // Сохраняем в JSON колонку
            // Убираем typeof() и здесь тоже
            r.Property(p => p.Name).IsRequired();
            r.Property(p => p.Description).IsRequired();
        });

        // --- НАСТРОЙКА ДОСТУПА К ПРИВАТНЫМ КОЛЛЕКЦИЯМ ---
        
        builder.Navigation(p => p.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}