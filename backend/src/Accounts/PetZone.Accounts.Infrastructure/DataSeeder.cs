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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountsDbContext>>();

        await SeedRolesAsync(roleManager, logger);
        await SeedPermissionsAsync(context, roleManager, logger);
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

        // Seed Permissions
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

        // Seed RolePermissions
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