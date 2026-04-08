using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class SystemNewsPostConfiguration : IEntityTypeConfiguration<SystemNewsPost>
{
    public void Configure(EntityTypeBuilder<SystemNewsPost> builder)
    {
        builder.ToTable("SystemNewsPosts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Type).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PublishedAt).IsRequired();
        builder.Property(p => p.LookingForHome).IsRequired();
        builder.Property(p => p.NeedsHelp).IsRequired();
        builder.Property(p => p.FoundHomeThisWeek).IsRequired();
        builder.Property(p => p.TotalVolunteers).IsRequired();
        builder.Property(p => p.FactEn).HasMaxLength(1000).IsRequired();
        builder.Property(p => p.TopBreedsJson).HasMaxLength(500).IsRequired();
        builder.Property(p => p.TopCity).HasMaxLength(100);
        builder.Property(p => p.FeaturedPetNickname).HasMaxLength(100);
        builder.Property(p => p.FeaturedPetPhotoUrl).HasMaxLength(500);
        builder.Property(p => p.FeaturedPetDescription).HasMaxLength(500);
        builder.Property(p => p.FeaturedPetBreed).HasMaxLength(100);
        builder.Property(p => p.FeaturedPetCity).HasMaxLength(100);
        builder.HasIndex(p => p.PublishedAt);
    }
}