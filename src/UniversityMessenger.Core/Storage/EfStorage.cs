using Microsoft.EntityFrameworkCore;
using UniversityMessenger.Core.Data;
using UniversityMessenger.Core.Models;

namespace UniversityMessenger.Core.Storage;

/// <summary>
/// Хранилище поверх SQLite через EF Core.
/// Реализует тот же контракт IStorage, что и InMemoryStorage.
/// </summary>
public class EfStorage : IStorage
{
    private readonly AppDbContext _db;

    public EfStorage(AppDbContext db)
    {
        _db = db;
    }

    // Пользователи
    public void AddUser(User user)
    {
        _db.Users.Add(user);
        _db.SaveChanges();
    }

    public User? GetUserById(Guid id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public User? GetUserByEmail(string email)
    {
        return _db.Users.FirstOrDefault(u => u.Email == email);
    }

    public List<User> GetUsers()
    {
        return _db.Users.ToList();
    }

    // Чаты. Include подгружает конверты ключей вместе с чатом.
    public void AddChat(Chat chat)
    {
        _db.Chats.Add(chat);
        _db.SaveChanges();
    }

    public Chat? GetChatById(Guid id)
    {
        return _db.Chats.Include(c => c.KeyWraps).FirstOrDefault(c => c.Id == id);
    }

    public Chat? GetDirectChatByKey(string directKey)
    {
        return _db.Chats.FirstOrDefault(c => c.DirectKey == directKey);
    }

    // Участники чатов
    public void AddChatMember(ChatMember member)
    {
        _db.ChatMembers.Add(member);
        _db.SaveChanges();
    }

    public List<ChatMember> GetMembersOfChat(Guid chatId)
    {
        return _db.ChatMembers.Where(m => m.ChatId == chatId).ToList();
    }

    public List<ChatMember> GetMembershipsOfUser(Guid userId)
    {
        return _db.ChatMembers.Where(m => m.UserId == userId).ToList();
    }

    public bool IsChatMember(Guid chatId, Guid userId)
    {
        return _db.ChatMembers.Any(m => m.ChatId == chatId && m.UserId == userId);
    }

    // Сообщения
    public void AddMessage(Message message)
    {
        _db.Messages.Add(message);
        _db.SaveChanges();
    }

    public List<Message> GetMessagesOfChat(Guid chatId)
    {
        return _db.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }
}
