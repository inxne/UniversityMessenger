using UniversityMessenger.Core.Models;

namespace UniversityMessenger.Core.Storage;

/// <summary>
/// Контракт хранилища. Сервисы работают только через него
/// и не знают, где лежат данные: в памяти или в базе.
/// </summary>
public interface IStorage
{
    // Пользователи
    void AddUser(User user);
    User? GetUserById(Guid id);
    User? GetUserByEmail(string email);
    List<User> GetUsers();

    // Чаты
    void AddChat(Chat chat);
    Chat? GetChatById(Guid id);
    Chat? GetDirectChatByKey(string directKey);

    // Участники чатов
    void AddChatMember(ChatMember member);
    List<ChatMember> GetMembersOfChat(Guid chatId);
    List<ChatMember> GetMembershipsOfUser(Guid userId);
    bool IsChatMember(Guid chatId, Guid userId);

    // Сообщения
    void AddMessage(Message message);
    List<Message> GetMessagesOfChat(Guid chatId);
}
