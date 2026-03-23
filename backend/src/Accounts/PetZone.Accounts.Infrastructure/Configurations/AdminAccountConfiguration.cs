using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure.Configurations;

public class AdminAccountConfiguration : IEntityTypeConfiguration<AdminAccount>
{
    public void Configure(EntityTypeBuilder<AdminAccount> builder)
    {
        builder.ToTable("admin_accounts");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.User)
            .WithOne(u => u.AdminAccount)
            .HasForeignKey<AdminAccount>(a => a.UserId);
    }
}