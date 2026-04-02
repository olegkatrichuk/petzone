using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PetZone.Species.Domain;

namespace PetZone.Species.Infrastructure.Configurations;

public class BreedConfiguration : IEntityTypeConfiguration<Breed>
{
    public void Configure(EntityTypeBuilder<Breed> builder)
    {
        builder.ToTable("Breeds");

        builder.HasKey(b => b.Id);

        var converter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)!);

        var comparer = new ValueComparer<Dictionary<string, string>>(
            (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
            c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
            c => JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);

        builder.Property(b => b.Translations)
            .HasColumnType("jsonb")
            .HasConversion(converter, comparer)
            .IsRequired();
    }
}
