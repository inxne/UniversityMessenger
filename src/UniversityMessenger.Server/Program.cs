using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UniversityMessenger.Core.Data;
using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;
using UniversityMessenger.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IStorage, EfStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddSignalR();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
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

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "UniversityMessenger.Server" }));

// --- Открытые эндпоинты ---

app.MapPost("/api/auth/register", (RegisterRequest req, AuthService auth) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(req.PublicKey))
            return Results.BadRequest(new { error = "Публичный ключ обязателен при регистрации." });

        return Results.Ok(new UserDto(auth.Register(req.Email, req.Password, req.FullName, req.Role, req.Faculty, req.Course, req.Consent, req.PublicKey)));
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

// --- Закрытые эндпоинты ---

app.MapGet("/api/users", (string? query, Role? role, string? faculty, int? course, UserService users) =>
    Results.Ok(users.Search(query, role, faculty, course).Select(u => new UserDto(u)))).RequireAuthorization();

app.MapGet("/api/users/{id:guid}/public-key", (Guid id, AuthService auth) =>
{
    try
    {
        var user = auth.GetById(id);
        return Results.Ok(new { userId = user.Id, publicKey = user.PublicKey });
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

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

// Создание группы: клиент присылает конверты ключа группы, по одному на участника.
// Сервер хранит конверты, но открыть не может ни один.
app.MapPost("/api/chats/group", (ClaimsPrincipal caller, GroupChatRequest req, ChatService chats) =>
{
    try
    {
        var wraps = req.KeyWraps.Select(k => new ChatKeyWrap { ForUserId = k.UserId, WrappedKey = k.WrappedKey }).ToList();
        return Results.Ok(new ChatDto(chats.CreateGroupChat(req.Name, caller.UserId(), req.MemberIds, wraps)));
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

// Участник забирает свой конверт ключа группы.
app.MapGet("/api/chats/{chatId:guid}/key-wraps", (Guid chatId, ClaimsPrincipal caller, ChatService chats) =>
{
    try
    {
        return Results.Ok(chats.GetKeyWrapsForUser(chatId, caller.UserId()).Select(w => new KeyWrapDto(w.ForUserId, w.WrappedKey)));
    }
    catch (AppException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.MapPost("/api/chats/{chatId:guid}/messages", async (Guid chatId, ClaimsPrincipal caller, SendCiphertextRequest req, ChatService chats, AuthService auth, IHubContext<ChatHub> hub) =>
{
    try
    {
        var senderId = caller.UserId();
        var message = chats.SendMessage(chatId, senderId, req.Ciphertext);
        var dto = new MessageDto(message, auth.GetById(senderId).FullName);
        await hub.Clients.Group(chatId.ToString()).SendAsync("MessageReceived", dto);
        return Results.Ok(dto);
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

app.MapHub<ChatHub>("/hubs/chat");

app.Run("http://localhost:5000");

// --- Помощник и генератор токенов ---

public static class ClaimsExtensions
{
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

public record RegisterRequest(string Email, string Password, string FullName, Role Role, string? Faculty, int? Course, bool Consent, string PublicKey);
public record LoginRequest(string Email, string Password);
public record DirectChatRequest(Guid OtherUserId);

// Конверт ключа группы для одного участника.
public record KeyWrapRequest(Guid UserId, string WrappedKey);

// Группа: название, участники и конверты ключа для каждого.
public record GroupChatRequest(string Name, List<Guid> MemberIds, List<KeyWrapRequest> KeyWraps);

public record SendCiphertextRequest(string Ciphertext);
public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);
public record KeyWrapDto(Guid ForUserId, string WrappedKey);

public record UserDto(Guid Id, string Email, string FullName, Role Role, string? Faculty, int? Course, string PublicKey, bool IsVerified, bool IsActive)
{
    public UserDto(User u) : this(u.Id, u.Email, u.FullName, u.Role, u.Faculty, u.Course, u.PublicKey, u.IsVerified, u.IsActive)
    {
    }
}

public record ChatDto(Guid Id, ChatType Type, string? Name, DateTime CreatedAt, string? Title = null)
{
    public ChatDto(Chat c, string? title = null) : this(c.Id, c.Type, c.Name, c.CreatedAt, title)
    {
    }
}

public record MessageDto(Guid Id, Guid ChatId, Guid SenderId, string SenderName, string Ciphertext, DateTime CreatedAt)
{
    public MessageDto(Message m, string senderName) : this(m.Id, m.ChatId, m.SenderId, senderName, m.Ciphertext, m.CreatedAt)
    {
    }
}
