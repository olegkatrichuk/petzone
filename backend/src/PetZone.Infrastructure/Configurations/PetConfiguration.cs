using System;
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
        
        // Эти свойства могут быть null, судя по знаку вопроса в типе (string?, Guid?)
        builder.Property(p => p.AdoptionConditions).IsRequired(false);
        builder.Property(p => p.MicrochipNumber).IsRequired(false);
        
        // Внешний ключ для связи с Volunteer (связь уже настроена со стороны волонтера, но здесь мы тоже мапим колонку)
        builder.Property(p => p.VolunteerId).IsRequired(false);

        // --- ОДИНОЧНЫЕ VALUE OBJECTS (с приватными свойствами) ---
        
        builder.OwnsOne(p => p.SpeciesBreedInfo, sb =>
        {
            sb.Property(typeof(Guid), "SpeciesId").HasColumnName("species_id").IsRequired();
            sb.Property(typeof(Guid), "BreedId").HasColumnName("breed_id").IsRequired();
        });

        builder.OwnsOne(p => p.Health, h =>
        {
            h.Property(typeof(string), "GeneralDescription").HasColumnName("health_general_description").IsRequired();
            h.Property(typeof(string), "DietOrAllergies").HasColumnName("health_diet_or_allergies").IsRequired(false);
        });

        builder.OwnsOne(p => p.Location, l =>
        {
            l.Property(typeof(string), "City").HasColumnName("city").IsRequired();
            l.Property(typeof(string), "Street").HasColumnName("street").IsRequired();
        });

        builder.OwnsOne(p => p.Weight, w =>
        {
            w.Property(typeof(double), "Value").HasColumnName("weight").IsRequired();
        });

        builder.OwnsOne(p => p.Height, h =>
        {
            h.Property(typeof(double), "Value").HasColumnName("height").IsRequired();
        });

        builder.OwnsOne(p => p.OwnerPhone, ph =>
        {
            ph.Property(typeof(string), "Value").HasColumnName("owner_phone").IsRequired();
        });

        // --- КОЛЛЕКЦИИ VALUE OBJECTS (JSON СТОЛБЦЫ) ---
        
        builder.OwnsMany(p => p.Requisites, r =>
        {
            r.ToJson(); // Сохраняем в JSON колонку
            r.Property(typeof(string), "Name").IsRequired();
            r.Property(typeof(string), "Description").IsRequired();
        });

        // --- НАСТРОЙКА ДОСТУПА К ПРИВАТНЫМ КОЛЛЕКЦИЯМ ---
        
        builder.Navigation(p => p.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}