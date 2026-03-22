using Microsoft.AspNetCore.Http.Features;
using PetZone.API.Middleware;
using PetZone.Species.Application;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Application;
using PetZone.Volunteers.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
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
builder.Services.AddSwaggerGen();

// Регистрируем контроллеры из модулей Presentation
builder.Services.AddControllers()
    .AddApplicationPart(typeof(PetZone.Volunteers.Presentation.VolunteersController).Assembly)
    .AddApplicationPart(typeof(PetZone.Species.Presentation.SpeciesController).Assembly);

// Модули
builder.Services.AddVolunteersApplication();
builder.Services.AddVolunteersInfrastructure(builder.Configuration);
builder.Services.AddSpeciesApplication();
builder.Services.AddSpeciesInfrastructure(builder.Configuration);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

var app = builder.Build();

// 2. Exception Middleware — первым в pipeline
app.UseMiddleware<ExceptionMiddleware>();

// 3. Логирование HTTP запросов
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }