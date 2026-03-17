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

        // ИСПОЛЬЗУЕМ КОНСТАНТУ ИЗ СУЩНОСТИ VOLUNTEER
        builder.Property(v => v.GeneralDescription)
               .HasMaxLength(Volunteer.MAX_GENERAL_DESCRIPTION_LENGTH)
               .IsRequired(false);

        // --- ОДИНОЧНЫЕ VALUE OBJECTS ---
        
        builder.ComplexProperty(v => v.Name, n => 
        {
            // ИСПОЛЬЗУЕМ КОНСТАНТЫ ИЗ FULLNAME
            n.Property(p => p.FirstName)
             .HasColumnName("first_name")
             .HasMaxLength(FullName.MAX_FIRST_NAME_LENGTH)
             .IsRequired();
             
            n.Property(p => p.LastName)
             .HasColumnName("last_name")
             .HasMaxLength(FullName.MAX_LAST_NAME_LENGTH)
             .IsRequired();
             
            n.Property(p => p.Patronymic)
             .HasColumnName("patronymic")
             .HasMaxLength(FullName.MAX_PATRONYMIC_LENGTH);
        });

        builder.ComplexProperty(v => v.Email, e => 
        {
            // ИСПОЛЬЗУЕМ КОНСТАНТУ ИЗ EMAIL
            e.Property(p => p.Value)
             .HasColumnName("email")
             .HasMaxLength(Email.MAX_LENGTH)
             .IsRequired();
        });

        builder.ComplexProperty(v => v.Experience, e => 
        {
            e.Property(p => p.Years).HasColumnName("experience_years").IsRequired();
        });

        builder.ComplexProperty(v => v.Phone, p => 
        {
            // ИСПОЛЬЗУЕМ КОНСТАНТУ ИЗ PHONENUMBER
            p.Property(p => p.Value)
             .HasColumnName("phone")
             .HasMaxLength(PhoneNumber.MAX_LENGTH)
             .IsRequired();
        });

        // --- КОЛЛЕКЦИИ VALUE OBJECTS (JSONB СТОЛБЦЫ) ---
        
        builder.OwnsMany(v => v.SocialNetworks, sn => 
        {
            sn.ToJson(); 
            // ИСПОЛЬЗУЕМ КОНСТАНТЫ ИЗ SOCIALNETWORK
            sn.Property(p => p.Name).HasMaxLength(SocialNetwork.MAX_NAME_LENGTH).IsRequired();
            sn.Property(p => p.Link).HasMaxLength(SocialNetwork.MAX_LINK_LENGTH).IsRequired();
        });

        builder.OwnsMany(v => v.Requisites, r => 
        {
            r.ToJson();
            // ИСПОЛЬЗУЕМ КОНСТАНТЫ ИЗ REQUISITE
            r.Property(p => p.Name).HasMaxLength(Requisite.MAX_NAME_LENGTH).IsRequired();
            r.Property(p => p.Description).HasMaxLength(Requisite.MAX_DESCRIPTION_LENGTH).IsRequired();
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