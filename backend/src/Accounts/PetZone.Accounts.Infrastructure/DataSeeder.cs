using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountsDbContext>>();

        await SeedRolesAsync(roleManager, logger);
        await SeedPermissionsAsync(context, roleManager, logger);
        await SeedAdminAsync(context, userManager, roleManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        string[] roles = [Role.Admin, Role.Volunteer, Role.Participant];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new Role { Name = role });
                logger.LogInformation("Role {Role} created", role);
            }
        }
    }

    private static async Task SeedAdminAsync(
        AccountsDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger logger)
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRST_NAME") ?? "Admin";
        var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LAST_NAME") ?? "PetZone";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("ADMIN_EMAIL or ADMIN_PASSWORD not set in environment");
            return;
        }

        // Перевіряємо чи адмін вже існує
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            logger.LogInformation("Admin already exists, skipping");
            return;
        }

        var adminRole = await roleManager.FindByNameAsync(Role.Admin);
        if (adminRole is null)
        {
            logger.LogWarning("Admin role not found");
            return;
        }

        var adminUser = User.CreateAdmin(adminEmail, adminFirstName, adminLastName, adminRole);

        // Створюємо user і AdminAccount в одній транзакції
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin: {Errors}", errors);
                await transaction.RollbackAsync();
                return;
            }

            await userManager.AddToRoleAsync(adminUser, Role.Admin);

            var adminAccount = new AdminAccount
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id
            };

            context.AdminAccounts.Add(adminAccount);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            logger.LogInformation("Admin user created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to create admin in transaction");
        }
    }

    private static async Task SeedPermissionsAsync(
        AccountsDbContext context,
        RoleManager<Role> roleManager,
        ILogger logger)
    {
        var jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "permissions.json");

        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("permissions.json not found at {Path}", jsonPath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var doc = JsonDocument.Parse(json);

        var allCodes = doc.RootElement
            .GetProperty("Permissions")
            .EnumerateObject()
            .SelectMany(g => g.Value.EnumerateArray().Select(v => v.GetString()!))
            .Distinct()
            .ToList();

        foreach (var code in allCodes)
        {
            if (!await context.Permissions.AnyAsync(p => p.Code == code))
            {
                context.Permissions.Add(new Permission { Id = Guid.NewGuid(), Code = code });
                logger.LogInformation("Permission {Code} created", code);
            }
        }

        await context.SaveChangesAsync();

        var rolePermissions = doc.RootElement.GetProperty("RolePermissions");

        foreach (var roleEntry in rolePermissions.EnumerateObject())
        {
            var roleName = roleEntry.Name;
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var permCodes = roleEntry.Value.EnumerateArray()
                .Select(v => v.GetString()!)
                .ToList();

            foreach (var code in permCodes)
            {
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == code);

                if (permission is null) continue;

                var exists = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (!exists)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Permissions seeded successfully");
    }
}