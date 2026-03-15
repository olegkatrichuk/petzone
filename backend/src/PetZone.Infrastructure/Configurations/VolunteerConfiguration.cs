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

        // --- ОДИНОЧНЫЕ VALUE OBJECTS (с приватными свойствами) ---
        
        builder.OwnsOne(v => v.Name, n => 
        {
            // Обращаемся к приватным свойствам через их имена в виде строк
            n.Property(typeof(string), "FirstName").IsRequired();
            n.Property(typeof(string), "LastName").IsRequired();
            n.Property(typeof(string), "Patronymic");
        });

        builder.OwnsOne(v => v.Email, e => 
        {
            e.Property(typeof(string), "Value").HasColumnName("email").IsRequired();
        });

        builder.OwnsOne(v => v.Experience, e => 
        {
            e.Property(typeof(int), "Years").HasColumnName("experience_years").IsRequired();
        });

        builder.OwnsOne(v => v.Phone, p => 
        {
            // Предполагаю, что в PhoneNumber свойство Value тоже стало приватным
            p.Property(typeof(string), "Value").HasColumnName("phone").IsRequired();
        });

        // --- КОЛЛЕКЦИИ VALUE OBJECTS (JSON СТОЛБЦЫ) ---
        
        builder.OwnsMany(v => v.SocialNetworks, sn => 
        {
            sn.ToJson();
            sn.Property(typeof(string), "Name").IsRequired();
            sn.Property(typeof(string), "Link").IsRequired();
        });

        builder.OwnsMany(v => v.Requisites, r => 
        {
            r.ToJson();
            // Предполагаю, что в Requisite свойства Name и Description
            r.Property(typeof(string), "Name").IsRequired();
            r.Property(typeof(string), "Description").IsRequired();
        });

        // --- СВЯЗЬ С ПИТОМЦАМИ ---
        builder.HasMany(v => v.Pets)
               .WithOne()
               .HasForeignKey("VolunteerId") // EF Core сам создаст теневое поле VolunteerId в таблице Pets
               .OnDelete(DeleteBehavior.Cascade);

        // --- УКАЗЫВАЕМ EF CORE ЧИТАТЬ ИЗ ПРИВАТНЫХ ПОЛЕЙ ---
        builder.Navigation(v => v.SocialNetworks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Pets).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}