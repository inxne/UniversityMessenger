using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;

// Собираем будущий сервер.
var builder = WebApplication.CreateBuilder(args);

// Регистрируем сервисы в контейнере зависимостей.
// Singleton означает: один экземпляр на всё приложение.
// Сервер сам создаст их и сам передаст куда нужно.
builder.Services.AddSingleton<IStorage, InMemoryStorage>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ChatService>();

// Swagger: автоматическая страница с документацией нашего API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Включаем страницу Swagger.
app.UseSwagger();
app.UseSwaggerUI();

// Проверка здоровья сервера: самый простой эндпоинт.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "UniversityMessenger.Server" }));

// Всегда слушаем один и тот же адрес, чтобы клиенты знали, куда стучаться.
app.Run("http://localhost:5000");
