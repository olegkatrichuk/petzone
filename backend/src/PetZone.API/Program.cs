using PetZone.Application.Repositories;
using PetZone.Application.Volunteers;
using PetZone.Infrastructure;
using PetZone.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем генерацию Swagger документации
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

// 2. Регистрируем БД и Контроллеры
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddControllers();

// 3. Регистрируем репозиторий (интерфейс -> реализация)
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();

// 4. Регистрируем сам сервис бизнес-логики
builder.Services.AddScoped<CreateVolunteerService>();

var app = builder.Build();

// 5. Включаем визуальный интерфейс Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// 6. САМОЕ ГЛАВНОЕ: Запускаем сервер, чтобы он не выключался!
app.Run();