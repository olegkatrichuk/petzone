using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Configurations;

public class DiscussionConfiguration : IEntityTypeConfiguration<Discussion>
{
    public void Configure(EntityTypeBuilder<Discussion> builder)
    {
        builder.ToTable("discussions");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.RelationId).IsRequired();
        builder.Property(d => d.IsClosed).IsRequired();

        builder.Property(d => d.Users)
            .HasColumnType("jsonb")
            .HasColumnName("users");

        builder.OwnsMany(d => d.Messages, m =>
        {
            m.ToTable("messages");
            m.HasKey(msg => msg.Id);
            m.Property(msg => msg.UserId).IsRequired();
            m.Property(msg => msg.Text).HasMaxLength(2000).IsRequired();
            m.Property(msg => msg.IsEdited).IsRequired();
            m.Property(msg => msg.CreatedAt).IsRequired();
        });
    }
}