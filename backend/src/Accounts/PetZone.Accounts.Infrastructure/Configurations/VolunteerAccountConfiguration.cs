using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure.Configurations;

public class VolunteerAccountConfiguration : IEntityTypeConfiguration<VolunteerAccount>
{
    public void Configure(EntityTypeBuilder<VolunteerAccount> builder)
    {
        builder.ToTable("volunteer_accounts");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Certificates)
            .HasColumnType("jsonb");

        builder.Property(v => v.Requisites)
            .HasColumnType("jsonb");

        builder.HasOne(v => v.User)
            .WithOne(u => u.VolunteerAccount)
            .HasForeignKey<VolunteerAccount>(v => v.UserId);
    }
}