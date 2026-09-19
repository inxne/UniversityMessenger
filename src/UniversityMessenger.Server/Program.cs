using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

// Сервисы регистрируем один раз, сервер сам раздаёт их эндпоинтам.
builder.Services.AddSingleton<IStorage, InMemoryStorage>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ChatService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Проверка здоровья сервера.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "UniversityMessenger.Server" }));

// --- Регистрация ---
app.MapPost("/api/auth/register", (RegisterRequest req, AuthService auth) =>
{
    try
    {
        var user = auth.Register(req.Email, req.Password, req.FullName, req.Role, req.Faculty, req.Course, req.Consent);
        return Results.Ok(new UserDto(user));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// --- Вход ---
app.MapPost("/api/auth/login", (LoginRequest req, AuthService auth) =>
{
    try
    {
        var user = auth.Login(req.Email, req.Password);
        return Results.Ok(new UserDto(user));
    }
    catch (AppException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 401);
    }
});

// --- Поиск пользователей ---
app.MapGet("/api/users", (string? query, Role? role, string? faculty, int? course, UserService users) =>
{
    var found = users.Search(query, role, faculty, course);
    return Results.Ok(found.Select(u => new UserDto(u)));
});

app.Run("http://localhost:5000");

// --- Контракты запросов и ответов ---

// То, что клиент присылает при регистрации.
public record RegisterRequest(string Email, string Password, string FullName, Role Role, string? Faculty, int? Course, bool Consent);

// То, что клиент присылает при входе.
public record LoginRequest(string Email, string Password);

// Публичные данные пользователя.
// PasswordHash наружу не отдаём никогда.
public record UserDto(Guid Id, string Email, string FullName, Role Role, string? Faculty, int? Course, bool IsVerified, bool IsActive)
{
    public UserDto(User u) : this(u.Id, u.Email, u.FullName, u.Role, u.Faculty, u.Course, u.IsVerified, u.IsActive)
    {
    }
}
