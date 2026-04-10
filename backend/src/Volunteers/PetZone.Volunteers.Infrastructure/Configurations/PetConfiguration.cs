using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nickname).HasMaxLength(Pet.MAX_NICKNAME_LENGTH).IsRequired();
        builder.Property(p => p.GeneralDescription).HasMaxLength(Pet.MAX_GENERAL_DESCRIPTION_LENGTH).IsRequired();
        builder.Property(p => p.Color).HasMaxLength(Pet.MAX_COLOR_LENGTH).IsRequired();

        builder.Property(p => p.AdoptionConditions).HasMaxLength(Pet.MAX_ADOPTION_CONDITIONS_LENGTH).IsRequired(false);
        builder.Property(p => p.MicrochipNumber).HasMaxLength(Pet.MAX_MICROCHIP_NUMBER_LENGTH).IsRequired(false);

        builder.Property(p => p.IsCastrated).IsRequired();
        builder.Property(p => p.DateOfBirth).IsRequired();
        builder.Property(p => p.IsVaccinated).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.VolunteerId).IsRequired(false);
        builder.Property(p => p.ExternalId).HasMaxLength(50).IsRequired(false);
        builder.HasIndex(p => p.ExternalId).IsUnique().HasFilter("external_id IS NOT NULL");
        builder.Property(p => p.IsDeleted).IsRequired();

        // Performance indexes for common filter/join patterns
        builder.HasIndex(p => p.VolunteerId);
        builder.HasIndex(p => p.IsDeleted);
        builder.HasIndex(p => new { p.VolunteerId, p.IsDeleted });
        builder.HasIndex(p => p.Status);
        builder.Property(p => p.DeletionDate).IsRequired(false);

        builder.ComplexProperty(p => p.SpeciesBreedInfo, sb =>
        {
            sb.Property(p => p.SpeciesId).HasColumnName("species_id").IsRequired();
            sb.Property(p => p.BreedId).HasColumnName("breed_id").IsRequired();
        });

        builder.ComplexProperty(p => p.Health, h =>
        {
            h.Property(p => p.GeneralDescription)
             .HasColumnName("health_general_description")
             .HasMaxLength(HealthInfo.MAX_GENERAL_DESCRIPTION_LENGTH)
             .IsRequired();

            h.Property(p => p.DietOrAllergies)
             .HasColumnName("health_diet_or_allergies")
             .HasMaxLength(HealthInfo.MAX_DIET_OR_ALLERGIES_LENGTH)
             .IsRequired(false);
        });

        builder.ComplexProperty(p => p.Location, l =>
        {
            l.Property(p => p.City)
             .HasColumnName("city")
             .HasMaxLength(Address.MAX_CITY_LENGTH)
             .IsRequired();

            l.Property(p => p.Street)
             .HasColumnName("street")
             .HasMaxLength(Address.MAX_STREET_LENGTH)
             .IsRequired();
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
            ph.Property(p => p.Value)
              .HasColumnName("owner_phone")
              .HasMaxLength(PhoneNumber.MAX_LENGTH)
              .IsRequired();
        });

        builder.OwnsMany(p => p.Requisites, r =>
        {
            r.ToJson();
            r.Property(p => p.Name).HasMaxLength(Requisite.MAX_NAME_LENGTH).IsRequired();
            r.Property(p => p.Description).HasMaxLength(Requisite.MAX_DESCRIPTION_LENGTH).IsRequired();
        });

        builder.OwnsMany(p => p.Photos, ph =>
        {
            ph.ToJson();
            ph.Property(p => p.FilePath)
                .HasMaxLength(PetPhoto.MAX_PATH_LENGTH)
                .IsRequired();
            ph.Property(p => p.IsMain).IsRequired();
        });

        builder.Navigation(p => p.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.Requisites).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}