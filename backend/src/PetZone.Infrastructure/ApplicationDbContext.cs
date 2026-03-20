using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PetZone.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Species;


namespace PetZone.Infrastructure;

public class ApplicationDbContext(IConfiguration configuration) : DbContext
{
    
    private const  string ConnectionStringName = "Database";
    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<Species> Species { get; set; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString(ConnectionStringName));
    }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Ручная конвертация в snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Проверяем, не является ли эта сущность частью JSON-колонки
            bool isJson = entity.IsMappedToJson();

            // Меняем имя таблицы только если это не JSON
            if (!isJson)
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entity.SetTableName(ToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                // Пропускаем скрытые первичные ключи у Value Objects
                if (entity.IsOwned() && property.IsPrimaryKey())
                    continue;

                // --- ИСПРАВЛЕНИЕ ОШИБКИ ЗДЕСЬ ---
                // Если свойство живет внутри JSON, пропускаем его (не задаем ColumnName)
                if (isJson)
                    continue;
                // --------------------------------

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