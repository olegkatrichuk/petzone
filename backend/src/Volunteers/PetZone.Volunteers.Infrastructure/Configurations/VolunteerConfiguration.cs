using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> builder)
    {
        builder.ToTable("Volunteers");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.UserId).IsRequired();
        builder.Property(v => v.IsDeleted).IsRequired();
        builder.Property(v => v.IsSystem).IsRequired().HasDefaultValue(false);

        // Performance indexes
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.IsDeleted);
        builder.Property(v => v.DeletionDate).IsRequired(false);

        builder.Property(v => v.GeneralDescription)
               .HasMaxLength(Volunteer.MaxGeneralDescriptionLength)
               .IsRequired(false);

        builder.Property(v => v.PhotoPath)
               .HasMaxLength(Volunteer.MaxPhotoPathLength)
               .IsRequired(false);

        builder.ComplexProperty(v => v.Name, n =>
        {
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
            p.Property(p => p.Value)
             .HasColumnName("phone")
             .HasMaxLength(PhoneNumber.MAX_LENGTH)
             .IsRequired();
        });

        builder.OwnsMany(v => v.SocialNetworks, sn =>
        {
            sn.ToJson();
            sn.Property(p => p.Name).HasMaxLength(SocialNetwork.MAX_NAME_LENGTH).IsRequired();
            sn.Property(p => p.Link).HasMaxLength(SocialNetwork.MAX_LINK_LENGTH).IsRequired();
        });

        builder.OwnsMany(v => v.Requisites, r =>
        {
            r.ToJson();
            r.Property(p => p.Name).HasMaxLength(Requisite.MAX_NAME_LENGTH).IsRequired();
            r.Property(p => p.Description).HasMaxLength(Requisite.MAX_DESCRIPTION_LENGTH).IsRequired();
        });

        builder.HasMany(v => v.Pets)
               .WithOne()
               .HasForeignKey("VolunteerId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.SocialNetworks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Pets).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}