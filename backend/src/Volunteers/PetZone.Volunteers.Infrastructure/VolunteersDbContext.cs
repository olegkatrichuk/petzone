using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure;

public class VolunteersDbContext(DbContextOptions<VolunteersDbContext> options) : DbContext(options)
{
    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<NewsPost> NewsPosts { get; set; }
    public DbSet<SystemNewsPost> SystemNewsPosts { get; set; }
    public DbSet<AdoptionApplication> AdoptionApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VolunteersDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            bool isJson = entity.IsMappedToJson();
            if (!isJson)
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entity.SetTableName(ToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                if (entity.IsOwned() && property.IsPrimaryKey()) continue;
                if (isJson) continue;
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                    property.SetColumnName(ToSnakeCase(columnName));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}