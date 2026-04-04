using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PetZone.Accounts.Application;
using PetZone.Accounts.Application.Accounts;
using PetZone.Accounts.Application.Accounts.ConfirmEmail;
using PetZone.Accounts.Application.Accounts.ForgotPassword;
using PetZone.Accounts.Application.Accounts.GetConfirmationLink;
using PetZone.Accounts.Application.Accounts.GetUserInfo;
using PetZone.Accounts.Application.Accounts.ResetPassword;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Accounts.Infrastructure.Cache;
using PetZone.Accounts.Infrastructure.Repositories;
using System.Text;

namespace PetZone.Accounts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AccountsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // Identity
        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AccountsDbContext>()
            .AddDefaultTokenProviders();

        // Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RefreshSessionOptions>(configuration.GetSection(RefreshSessionOptions.SectionName));

        // Providers
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        // Services
        services.AddScoped<RegisterUserService>();
        services.AddScoped<LoginUserService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<GetUserInfoService>();
        services.AddScoped<GetConfirmationLinkService>();
        services.AddScoped<ConfirmEmailService>();
        services.AddScoped<ForgotPasswordService>();
        services.AddScoped<ResetPasswordService>();

        // Repositories & UnitOfWork
        services.AddScoped<IAccountsUnitOfWork, AccountsUnitOfWork>();
        services.AddScoped<IParticipantAccountRepository, ParticipantAccountRepository>();
        services.AddScoped<IRefreshSessionRepository, RedisRefreshSessionRepository>();

        // MediatR (cache invalidation handlers)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(UserCacheInvalidationHandler).Assembly));

        // JWT Authentication
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        // Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();
        // MassTransit
        services.AddMassTransit(x =>
        {
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