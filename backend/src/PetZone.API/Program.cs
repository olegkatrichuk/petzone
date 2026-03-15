using PetZone.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<ApplicationDbContext>();


var app = builder.Build();

app.UseHttpsRedirection();
