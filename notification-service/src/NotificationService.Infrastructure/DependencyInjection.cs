using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application;
using NotificationService.Application.Commands.UpsertNotificationSettings;
using NotificationService.Infrastructure.Consumers;
using NotificationService.Infrastructure.Email;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<INotificationDbContext>(sp =>
            sp.GetRequiredService<NotificationDbContext>());

        services.AddScoped<UpsertNotificationSettingsHandler>();

        // Email
        services.AddScoped<IEmailSender, GmailEmailSender>();

        // HTTP Client
        services.AddHttpClient("AccountsApi", client =>
        {
            client.BaseAddress = new Uri(
                configuration["AccountsApi:BaseUrl"] ?? "http://localhost:5183/");
        });

        services.AddMassTransit(x =>
        {
            x.AddConsumer<VolunteerRequestStatusChangedConsumer>();
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<ListingCreatedConsumer>();
            x.AddConsumer<ListingAdoptedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost",
                    ushort.Parse(configuration["RabbitMq:Port"] ?? "5672"),
                    "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}