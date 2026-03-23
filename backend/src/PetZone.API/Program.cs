using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi;
using PetZone.Accounts.Infrastructure;
using PetZone.API.Middleware;
using PetZone.Species.Application;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Application;
using PetZone.Volunteers.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq(
            context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
            apiKey: context.Configuration["Seq:ApiKey"]);
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
    .AddApplicationPart(typeof(PetZone.Accounts.Presentation.AccountsController).Assembly);

builder.Services.AddVolunteersApplication();
builder.Services.AddVolunteersInfrastructure(builder.Configuration);
builder.Services.AddSpeciesApplication();
builder.Services.AddSpeciesInfrastructure(builder.Configuration);
builder.Services.AddAccountsInfrastructure(builder.Configuration);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }