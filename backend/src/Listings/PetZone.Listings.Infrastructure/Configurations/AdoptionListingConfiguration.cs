using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure.Configurations;

public class AdoptionListingConfiguration : IEntityTypeConfiguration<AdoptionListing>
{
    public void Configure(EntityTypeBuilder<AdoptionListing> builder)
    {
        builder.ToTable("adoption_listings");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.UserId).IsRequired();
        builder.Property(l => l.UserName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.UserEmail).HasMaxLength(300).IsRequired();
        builder.Property(l => l.UserPhone).HasMaxLength(30);
        builder.Property(l => l.ContactEmail).HasMaxLength(256);
        builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(2000).IsRequired();
        builder.Property(l => l.SpeciesId).IsRequired();
        builder.Property(l => l.BreedId);
        builder.Property(l => l.AgeMonths).IsRequired();
        builder.Property(l => l.Color).HasMaxLength(100).IsRequired();
        builder.Property(l => l.City).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Vaccinated).IsRequired();
        builder.Property(l => l.Castrated).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired();

        var photosConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var photosComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(l => l.Photos)
            .HasColumnType("jsonb")
            .HasConversion(photosConverter, photosComparer)
            .IsRequired();
    }
}
