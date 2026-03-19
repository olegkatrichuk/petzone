using Microsoft.Extensions.DependencyInjection;
using PetZone.Infrastructure.BackgroundServices;
using PetZone.Infrastructure.Options;
using PetZone.Infrastructure.Repositories;
using PetZone.UseCases.Repositories;

namespace PetZone.Infrastructure;

public static class DependencyInjection
{
    // Метод расширения для IServiceCollection
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Регистрируем контекст базы данных
        services.AddScoped<ApplicationDbContext>();

        // Регистрируем все репозитории
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        
        // Конфигурация soft delete
        services.AddOptions<SoftDeleteOptions>()
            .BindConfiguration(SoftDeleteOptions.SectionName);

        // Background service для очистки удалённых сущностей
        services.AddHostedService<SoftDeleteCleanupService>();

        // Если в будущем появятся другие сервисы инфраструктуры 
        // (например, работа с S3 для картинок), мы добавим их сюда.

        return services;
    }
}