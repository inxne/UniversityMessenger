using Microsoft.EntityFrameworkCore;
using UniversityMessenger.Core.Models;

namespace UniversityMessenger.Core.Data;

/// <summary>
/// Контекст базы данных: описание таблиц и правил схемы.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ChatKeyWrap> ChatKeyWraps => Set<ChatKeyWrap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Почта уникальна: двух аккаунтов с одной почтой быть не может.
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Ключ личного чата уникален: одна пара людей имеет один личный чат.
        modelBuilder.Entity<Chat>().HasIndex(c => c.DirectKey).IsUnique();

        // У участника ровно один конверт ключа на чат.
        modelBuilder.Entity<ChatKeyWrap>().HasIndex(w => new { w.ChatId, w.ForUserId }).IsUnique();
    }
}
