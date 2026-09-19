using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, InMemoryStorage>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ChatService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "UniversityMessenger.Server" }));

// --- Регистрация и вход ---
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

// --- Личный чат: создать или получить существующий ---
app.MapPost("/api/chats/direct", (DirectChatRequest req, ChatService chats) =>
{
    try
    {
        var chat = chats.GetOrCreateDirectChat(req.User1Id, req.User2Id);
        return Results.Ok(new ChatDto(chat));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// --- Групповой чат ---
app.MapPost("/api/chats/group", (GroupChatRequest req, ChatService chats) =>
{
    try
    {
        var chat = chats.CreateGroupChat(req.Name, req.CreatorId, req.MemberIds);
        return Results.Ok(new ChatDto(chat));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// --- Список чатов пользователя с заголовками ---
app.MapGet("/api/chats", (Guid userId, ChatService chats) =>
{
    var list = chats.GetChatsOfUser(userId);
    var result = list.Select(c => new ChatDto(c, chats.GetChatTitle(c.Id, userId)));
    return Results.Ok(result);
});

// --- Отправить сообщение ---
app.MapPost("/api/chats/{chatId:guid}/messages", (Guid chatId, SendMessageRequest req, ChatService chats, AuthService auth) =>
{
    try
    {
        var message = chats.SendMessage(chatId, req.SenderId, req.Text);
        var sender = auth.GetById(req.SenderId);
        return Results.Ok(new MessageDto(message, sender.FullName));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// --- История сообщений чата ---
app.MapGet("/api/chats/{chatId:guid}/messages", (Guid chatId, Guid userId, ChatService chats, AuthService auth) =>
{
    try
    {
        var history = chats.GetHistory(chatId, userId);
        var result = history.Select(m => new MessageDto(m, auth.GetById(m.SenderId).FullName));
        return Results.Ok(result);
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run("http://localhost:5000");

// --- Контракты запросов и ответов ---

public record RegisterRequest(string Email, string Password, string FullName, Role Role, string? Faculty, int? Course, bool Consent);
public record LoginRequest(string Email, string Password);
public record DirectChatRequest(Guid User1Id, Guid User2Id);
public record GroupChatRequest(string Name, Guid CreatorId, List<Guid> MemberIds);
public record SendMessageRequest(Guid SenderId, string Text);

// Публичные данные пользователя, без PasswordHash.
public record UserDto(Guid Id, string Email, string FullName, Role Role, string? Faculty, int? Course, bool IsVerified, bool IsActive)
{
    public UserDto(User u) : this(u.Id, u.Email, u.FullName, u.Role, u.Faculty, u.Course, u.IsVerified, u.IsActive)
    {
    }
}

// Публичные данные чата. Title заполняется в списке чатов:
// для группы это название, для личного чата это ФИО собеседника.
public record ChatDto(Guid Id, ChatType Type, string? Name, DateTime CreatedAt, string? Title = null)
{
    public ChatDto(Chat c, string? title = null) : this(c.Id, c.Type, c.Name, c.CreatedAt, title)
    {
    }
}

// Сообщение с именем отправителя, чтобы клиент не дёргал пользователей по одному.
public record MessageDto(Guid Id, Guid ChatId, Guid SenderId, string SenderName, string Text, DateTime CreatedAt)
{
    public MessageDto(Message m, string senderName) : this(m.Id, m.ChatId, m.SenderId, senderName, m.Text, m.CreatedAt)
    {
    }
}
