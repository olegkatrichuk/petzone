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
        builder.HasIndex(p => p.PublishedAt);
    }
}