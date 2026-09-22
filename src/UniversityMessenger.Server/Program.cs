using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, InMemoryStorage>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

// Проверяем присланные токены: подпись, издатель, адресат, срок.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Кнопка Authorize в Swagger: вставил токен и тестируешь закрытые эндпоинты.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставьте токен из ответа /api/auth/login"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "UniversityMessenger.Server" }));

// --- Открытые эндпоинты ---

app.MapPost("/api/auth/register", (RegisterRequest req, AuthService auth) =>
{
    try
    {
        return Results.Ok(new UserDto(auth.Register(req.Email, req.Password, req.FullName, req.Role, req.Faculty, req.Course, req.Consent)));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/auth/login", (LoginRequest req, AuthService auth, TokenService tokens) =>
{
    try
    {
        var user = auth.Login(req.Email, req.Password);
        return Results.Ok(new LoginResponse(tokens.Generate(user), DateTime.UtcNow.AddHours(8), new UserDto(user)));
    }
    catch (AppException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 401);
    }
});

// --- Закрытые эндпоинты: кто ты, сервер знает из токена ---

app.MapGet("/api/users", (string? query, Role? role, string? faculty, int? course, UserService users) =>
    Results.Ok(users.Search(query, role, faculty, course).Select(u => new UserDto(u)))).RequireAuthorization();

app.MapPost("/api/chats/direct", (ClaimsPrincipal caller, DirectChatRequest req, ChatService chats) =>
{
    try
    {
        return Results.Ok(new ChatDto(chats.GetOrCreateDirectChat(caller.UserId(), req.OtherUserId)));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.MapPost("/api/chats/group", (ClaimsPrincipal caller, GroupChatRequest req, ChatService chats) =>
{
    try
    {
        return Results.Ok(new ChatDto(chats.CreateGroupChat(req.Name, caller.UserId(), req.MemberIds)));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.MapGet("/api/chats", (ClaimsPrincipal caller, ChatService chats) =>
{
    var id = caller.UserId();
    return Results.Ok(chats.GetChatsOfUser(id).Select(c => new ChatDto(c, chats.GetChatTitle(c.Id, id))));
}).RequireAuthorization();

app.MapPost("/api/chats/{chatId:guid}/messages", (Guid chatId, ClaimsPrincipal caller, SendMessageRequest req, ChatService chats, AuthService auth) =>
{
    try
    {
        var senderId = caller.UserId();
        var message = chats.SendMessage(chatId, senderId, req.Text);
        return Results.Ok(new MessageDto(message, auth.GetById(senderId).FullName));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.MapGet("/api/chats/{chatId:guid}/messages", (Guid chatId, ClaimsPrincipal caller, ChatService chats, AuthService auth) =>
{
    try
    {
        return Results.Ok(chats.GetHistory(chatId, caller.UserId()).Select(m => new MessageDto(m, auth.GetById(m.SenderId).FullName)));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.Run("http://localhost:5000");

// --- Помощник и генератор токенов ---

public static class ClaimsExtensions
{
    // Достаём Id пользователя прямо из токена.
    public static Guid UserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class TokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        _key = config["Jwt:Key"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
    }

    public string Generate(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// --- Контракты ---

public record RegisterRequest(string Email, string Password, string FullName, Role Role, string? Faculty, int? Course, bool Consent);
public record LoginRequest(string Email, string Password);
public record DirectChatRequest(Guid OtherUserId);
public record GroupChatRequest(string Name, List<Guid> MemberIds);
public record SendMessageRequest(string Text);
public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(Guid Id, string Email, string FullName, Role Role, string? Faculty, int? Course, bool IsVerified, bool IsActive)
{
    public UserDto(User u) : this(u.Id, u.Email, u.FullName, u.Role, u.Faculty, u.Course, u.IsVerified, u.IsActive)
    {
    }
}

public record ChatDto(Guid Id, ChatType Type, string? Name, DateTime CreatedAt, string? Title = null)
{
    public ChatDto(Chat c, string? title = null) : this(c.Id, c.Type, c.Name, c.CreatedAt, title)
    {
    }
}

public record MessageDto(Guid Id, Guid ChatId, Guid SenderId, string SenderName, string Text, DateTime CreatedAt)
{
    public MessageDto(Message m, string senderName) : this(m.Id, m.ChatId, m.SenderId, senderName, m.Text, m.CreatedAt)
    {
    }
}

