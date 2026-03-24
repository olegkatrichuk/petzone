using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Configurations;

public class VolunteerRequestConfiguration : IEntityTypeConfiguration<VolunteerRequest>
{
    public void Configure(EntityTypeBuilder<VolunteerRequest> builder)
    {
        builder.ToTable("volunteer_requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.AdminId);
        builder.Property(r => r.DiscussionId);
        builder.Property(r => r.Status).HasConversion<string>();
        builder.Property(r => r.RejectionComment);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.OwnsOne(r => r.VolunteerInfo, vi =>
        {
            vi.Property(v => v.Experience).HasColumnName("experience");
            vi.Property(v => v.Certificates).HasColumnType("jsonb").HasColumnName("certificates");
            vi.Property(v => v.Requisites).HasColumnType("jsonb").HasColumnName("requisites");
        });
    }
}