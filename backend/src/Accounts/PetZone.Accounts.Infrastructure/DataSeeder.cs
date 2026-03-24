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
        // 1 запит — завантажуємо всі існуючі ролі одразу
        var existingRoles = await roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync();

        string[] roles = [Role.Admin, Role.Volunteer, Role.Participant];

        foreach (var role in roles.Except(existingRoles))
        {
            await roleManager.CreateAsync(new Role { Name = role });
            logger.LogInformation("Role {Role} created", role);
        }
    }

    private static async Task SeedPermissionsAsync(
        AccountsDbContext context,
        RoleManager<Role> roleManager,
        ILogger logger)
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "permissions.json");

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

        // 1 запит — завантажуємо всі існуючі permissions
        var existingPermissions = await context.Permissions.ToListAsync();
        var existingCodes = existingPermissions.Select(p => p.Code).ToHashSet();

        var newPermissions = allCodes
            .Where(code => !existingCodes.Contains(code))
            .Select(code => new Permission { Id = Guid.NewGuid(), Code = code })
            .ToList();

        if (newPermissions.Count > 0)
        {
            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();
            existingPermissions.AddRange(newPermissions);
        }

        // 1 запит — завантажуємо всі існуючі role-permissions
        var existingRolePermissions = await context.RolePermissions.ToListAsync();
        var existingRolePermissionSet = existingRolePermissions
            .Select(rp => (rp.RoleId, rp.PermissionId))
            .ToHashSet();

        // 1 запит — завантажуємо всі ролі
        var allRoles = await roleManager.Roles.ToListAsync();
        var rolesByName = allRoles.ToDictionary(r => r.Name!, r => r);
        var permissionsByCode = existingPermissions.ToDictionary(p => p.Code, p => p);

        var rolePermissionsToAdd = new List<RolePermission>();
        var rolePermissions = doc.RootElement.GetProperty("RolePermissions");

        foreach (var roleEntry in rolePermissions.EnumerateObject())
        {
            if (!rolesByName.TryGetValue(roleEntry.Name, out var role))
                continue;

            var permCodes = roleEntry.Value
                .EnumerateArray()
                .Select(v => v.GetString()!)
                .ToList();

            foreach (var code in permCodes)
            {
                if (!permissionsByCode.TryGetValue(code, out var permission))
                    continue;

                if (!existingRolePermissionSet.Contains((role.Id, permission.Id)))
                {
                    rolePermissionsToAdd.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        if (rolePermissionsToAdd.Count > 0)
        {
            context.RolePermissions.AddRange(rolePermissionsToAdd);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Permissions seeded successfully");
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

            context.AdminAccounts.Add(new AdminAccount
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id
            });

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
}