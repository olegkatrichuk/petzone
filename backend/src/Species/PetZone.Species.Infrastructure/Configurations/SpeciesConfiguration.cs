using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpeciesEntity = PetZone.Species.Domain.Species;

namespace PetZone.Species.Infrastructure.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<SpeciesEntity>
{
    public void Configure(EntityTypeBuilder<SpeciesEntity> builder)
    {
        builder.ToTable("Species");

        builder.HasKey(s => s.Id);

        var converter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)!);

        var comparer = new ValueComparer<Dictionary<string, string>>(
            (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
            c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
            c => JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);

        builder.Property(s => s.Translations)
            .HasColumnType("jsonb")
            .HasConversion(converter, comparer)
            .IsRequired();

        builder.HasMany(s => s.Breeds)
            .WithOne()
            .HasForeignKey("SpeciesId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Breeds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}