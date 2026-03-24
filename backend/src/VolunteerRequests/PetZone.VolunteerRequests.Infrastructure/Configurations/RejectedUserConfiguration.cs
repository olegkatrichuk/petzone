using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Configurations;

public class RejectedUserConfiguration : IEntityTypeConfiguration<RejectedUser>
{
    public void Configure(EntityTypeBuilder<RejectedUser> builder)
    {
        builder.ToTable("rejected_users");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.RejectedUntil).IsRequired();
    }
}