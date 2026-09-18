using UniversityMessenger.Core.Models;

namespace UniversityMessenger.Core.Storage;

/// <summary>
/// Хранилище в оперативной памяти для демо.
/// Позже заменим на базу данных, сервисы этого не заметят.
/// </summary>
public class InMemoryStorage : IStorage
{
    // Наши "таблицы" в памяти.
    private readonly List<User> _users = new();
    private readonly List<Chat> _chats = new();
    private readonly List<ChatMember> _members = new();
    private readonly List<Message> _messages = new();

    // Пользователи
    public void AddUser(User user)
    {
        _users.Add(user);
    }

    public User? GetUserById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public User? GetUserByEmail(string email)
    {
        return _users.FirstOrDefault(u => u.Email == email);
    }

    public List<User> GetUsers()
    {
        // Копия, чтобы снаружи список нельзя было испортить.
        return new List<User>(_users);
    }

    // Чаты
    public void AddChat(Chat chat)
    {
        _chats.Add(chat);
    }

    public Chat? GetChatById(Guid id)
    {
        return _chats.FirstOrDefault(c => c.Id == id);
    }

    public Chat? GetDirectChatByKey(string directKey)
    {
        return _chats.FirstOrDefault(c => c.DirectKey == directKey);
    }

    // Участники чатов
    public void AddChatMember(ChatMember member)
    {
        _members.Add(member);
    }

    public List<ChatMember> GetMembersOfChat(Guid chatId)
    {
        return _members.Where(m => m.ChatId == chatId).ToList();
    }

    public List<ChatMember> GetMembershipsOfUser(Guid userId)
    {
        return _members.Where(m => m.UserId == userId).ToList();
    }

    public bool IsChatMember(Guid chatId, Guid userId)
    {
        return _members.Any(m => m.ChatId == chatId && m.UserId == userId);
    }

    // Сообщения
    public void AddMessage(Message message)
    {
        _messages.Add(message);
    }

    public List<Message> GetMessagesOfChat(Guid chatId)
    {
        return _messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }
}
