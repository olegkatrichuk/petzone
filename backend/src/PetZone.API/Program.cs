using System.Threading.RateLimiting;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using PetZone.Accounts.Infrastructure;
using PetZone.Framework.Cache;
using StackExchange.Redis;
using PetZone.API.Middleware;
using PetZone.Species.Application;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Application;
using PetZone.Volunteers.Infrastructure;
using PetZone.VolunteerRequests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Application;
using PetZone.Listings.Infrastructure;
using Serilog;

DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, "../../../../../.env"));
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    var elasticsearchUrl = context.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200";

    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq(
            context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
            apiKey: context.Configuration["Seq:ApiKey"])
        .WriteTo.Elasticsearch(
            [new Uri(elasticsearchUrl)],
            opts =>
            {
                opts.DataStream = new DataStreamName("logs", "petzone", "default");
                opts.BootstrapMethod = BootstrapMethod.Silent;
            });
});

var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = redisConnectionString);
builder.Services.AddScoped<PetZone.Core.ICacheService, CacheService>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddControllers()
    .AddApplicationPart(typeof(PetZone.Volunteers.Presentation.VolunteersController).Assembly)
    .AddApplicationPart(typeof(PetZone.Species.Presentation.SpeciesController).Assembly)
    .AddApplicationPart(typeof(PetZone.Accounts.Presentation.AccountsController).Assembly)
    .AddApplicationPart(typeof(PetZone.VolunteerRequests.Presentation.VolunteerRequestsController).Assembly)
    .AddApplicationPart(typeof(PetZone.Listings.Presentation.ListingsController).Assembly);

builder.Services.AddVolunteersApplication();
builder.Services.AddVolunteersInfrastructure(builder.Configuration);
builder.Services.AddSpeciesApplication();
builder.Services.AddSpeciesInfrastructure(builder.Configuration);
builder.Services.AddAccountsInfrastructure(builder.Configuration);
builder.Services.AddVolunteerRequestsInfrastructure(builder.Configuration);
builder.Services.AddListingsApplication();
builder.Services.AddListingsInfrastructure(builder.Configuration);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // login: 10 attempts per minute per IP
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });

    // forgot-password: 3 attempts per 5 minutes per IP
    options.AddFixedWindowLimiter("forgot-password", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
});

var dbConnectionString = builder.Configuration.GetConnectionString("Database")!;
builder.Services.AddHealthChecks()
    .AddNpgSql(dbConnectionString, name: "postgres", tags: ["db"])
    .AddRedis(redisConnectionString, name: "redis", tags: ["cache"]);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<AccountsDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<VolunteersDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<SpeciesDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<VolunteerRequestsDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<ListingsDbContext>().Database.MigrateAsync();
}

await DataSeeder.SeedAsync(app.Services);
await PetZone.Species.Infrastructure.SpeciesSeeder.SeedAsync(app.Services);
await PetZone.Volunteers.Infrastructure.VolunteersSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }