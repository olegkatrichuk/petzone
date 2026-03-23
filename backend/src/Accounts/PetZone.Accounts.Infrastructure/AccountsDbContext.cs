using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetZone.Accounts.Domain;
using PetZone.Accounts.Infrastructure.Configurations;

namespace PetZone.Accounts.Infrastructure;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ParticipantAccount> ParticipantAccounts => Set<ParticipantAccount>();
    public DbSet<VolunteerAccount> VolunteerAccounts => Set<VolunteerAccount>();
    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("accounts");
        builder.ApplyConfiguration(new PermissionConfiguration());
        builder.ApplyConfiguration(new RolePermissionConfiguration());
        builder.ApplyConfiguration(new ParticipantAccountConfiguration());
        builder.ApplyConfiguration(new VolunteerAccountConfiguration());
        builder.ApplyConfiguration(new AdminAccountConfiguration());
    }
}