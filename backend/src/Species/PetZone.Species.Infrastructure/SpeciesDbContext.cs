using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace PetZone.Species.Infrastructure;

public class SpeciesDbContext(DbContextOptions<SpeciesDbContext> options) : DbContext(options)
{
    public DbSet<PetZone.Species.Domain.Species> Species { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SpeciesDbContext).Assembly);

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