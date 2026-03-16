using Microsoft.Extensions.DependencyInjection;
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

        // Если в будущем появятся другие сервисы инфраструктуры 
        // (например, работа с S3 для картинок), мы добавим их сюда.

        return services;
    }
}