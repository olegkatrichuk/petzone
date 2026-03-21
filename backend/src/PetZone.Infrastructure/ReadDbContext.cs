using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetZone.Domain.Models;
using System.Text.RegularExpressions;

namespace PetZone.Infrastructure;

public class ReadDbContext : DbContext
{
    private readonly IConfiguration? _configuration;
    private const string ConnectionStringName = "Database";

    // Конструктор для production
    public ReadDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Конструктор для тестов
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<PetZone.Domain.Species.Species> Species { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration != null)
        {
            optionsBuilder
                .UseNpgsql(_configuration.GetConnectionString(ConnectionStringName))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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
                if (entity.IsOwned() && property.IsPrimaryKey())
                    continue;

                if (isJson)
                    continue;

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