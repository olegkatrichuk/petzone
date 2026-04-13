using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class AdoptionApplicationConfiguration : IEntityTypeConfiguration<AdoptionApplication>
{
    public void Configure(EntityTypeBuilder<AdoptionApplication> builder)
    {
        builder.ToTable("AdoptionApplications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PetId).IsRequired();
        builder.Property(a => a.VolunteerId).IsRequired();
        builder.Property(a => a.ApplicantUserId).IsRequired();
        builder.Property(a => a.ApplicantName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ApplicantPhone).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Message).HasMaxLength(1000).IsRequired(false);
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.VolunteerId);
        builder.HasIndex(a => a.ApplicantUserId);
        builder.HasIndex(a => new { a.PetId, a.ApplicantUserId }).IsUnique();
    }
}