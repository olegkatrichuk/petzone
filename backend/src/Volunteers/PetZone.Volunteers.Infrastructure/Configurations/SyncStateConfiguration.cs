using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class SyncStateConfiguration : IEntityTypeConfiguration<SyncState>
{
    public void Configure(EntityTypeBuilder<SyncState> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ServiceName).HasMaxLength(SyncState.MaxServiceNameLength).IsRequired();
        builder.Property(s => s.LastRunAt).IsRequired();
        builder.HasIndex(s => s.ServiceName).IsUnique();
    }
}
