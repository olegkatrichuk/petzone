using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Domain.Models;

namespace PetZone.Infrastructure.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> builder)
    {
        builder.ToTable("Volunteers");
        
        builder.HasKey(v => v.Id);

        builder.Property(v => v.GeneralDescription).IsRequired(false);

        // --- ОДИНОЧНЫЕ VALUE OBJECTS (Используем новый ComplexProperty!) ---
        
        builder.ComplexProperty(v => v.Name, n => 
        {
            // Теперь всё строго типизировано, без строк!
            n.Property(p => p.FirstName).HasColumnName("first_name").IsRequired();
            n.Property(p => p.LastName).HasColumnName("last_name").IsRequired();
            n.Property(p => p.Patronymic).HasColumnName("patronymic");
        });

        builder.ComplexProperty(v => v.Email, e => 
        {
            e.Property(p => p.Value).HasColumnName("email").IsRequired();
        });

        builder.ComplexProperty(v => v.Experience, e => 
        {
            e.Property(p => p.Years).HasColumnName("experience_years").IsRequired();
        });

        builder.ComplexProperty(v => v.Phone, p => 
        {
            p.Property(p => p.Value).HasColumnName("phone").IsRequired();
        });

        // --- КОЛЛЕКЦИИ VALUE OBJECTS (JSONB СТОЛБЦЫ) ---
        
        builder.OwnsMany(v => v.SocialNetworks, sn => 
        {
            sn.ToJson(); // Магия JSONB
            // Здесь тоже избавляемся от строковых имен свойств
            sn.Property(p => p.Name).IsRequired();
            sn.Property(p => p.Link).IsRequired();
        });

        builder.OwnsMany(v => v.Requisites, r => 
        {
            r.ToJson();
            r.Property(p => p.Name).IsRequired();
            r.Property(p => p.Description).IsRequired();
        });

        // --- СВЯЗЬ С ПИТОМЦАМИ ---
        builder.HasMany(v => v.Pets)
               .WithOne()
               .HasForeignKey("VolunteerId")
               .OnDelete(DeleteBehavior.Cascade);

        // --- УКАЗЫВАЕМ EF CORE ЧИТАТЬ ИЗ ПРИВАТНЫХ ПОЛЕЙ ---
        builder.Navigation(v => v.SocialNetworks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Pets).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}