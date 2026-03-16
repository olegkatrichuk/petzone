
using PetZone.Infrastructure;
using PetZone.UseCases;
using PetZone.UseCases.Volunteers;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем генерацию Swagger документации
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

// 2. Регистрируем Контроллеры (API слой)
builder.Services.AddControllers();

// 3. Подключаем внешние слои (вся магия регистрации БД и репозиториев теперь скрыта тут)
builder.Services.AddInfrastructure();

// 4. Подключаем слой бизнес-логики
builder.Services.AddApplication();

var app = builder.Build();

// 5. Включаем визуальный интерфейс Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();