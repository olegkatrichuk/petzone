using PetZone.API.Middleware;
using PetZone.Infrastructure;
using PetZone.UseCases;
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
builder.Services.AddControllers();
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

var app = builder.Build();

// 2. Exception Middleware — первым в pipeline
app.UseMiddleware<ExceptionMiddleware>();

// 3. Логирование HTTP запросов
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
    
    //применить миграции
    
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();