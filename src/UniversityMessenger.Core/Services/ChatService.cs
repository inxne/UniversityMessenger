using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Storage;

namespace UniversityMessenger.Core.Services;

/// <summary>
/// Чаты, участники и сообщения.
/// </summary>
public class ChatService
{
    private readonly IStorage _storage;

    public ChatService(IStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Личный чат: возвращает существующий или создаёт новый.
    /// DirectKey гарантирует, что пара людей имеет ровно один личный чат.
    /// </summary>
    public Chat GetOrCreateDirectChat(Guid user1Id, Guid user2Id)
    {
        if (user1Id == user2Id)
            throw new AppException("Нельзя создать чат с самим собой.");

        var key = MakeDirectKey(user1Id, user2Id);

        var existing = _storage.GetDirectChatByKey(key);
        if (existing != null)
            return existing;

        var chat = new Chat { Type = ChatType.Direct, DirectKey = key };
        _storage.AddChat(chat);

        _storage.AddChatMember(new ChatMember { ChatId = chat.Id, UserId = user1Id });
        _storage.AddChatMember(new ChatMember { ChatId = chat.Id, UserId = user2Id });

        return chat;
    }

    /// <summary>
    /// Ключ личного чата: два Id, отсортированных по возрастанию, через двоеточие.
    /// Сортировка нужна, чтобы пара A-B и пара B-A давали один и тот же ключ.
    /// </summary>
    private static string MakeDirectKey(Guid a, Guid b)
    {
        var ids = new[] { a, b }.OrderBy(x => x).ToArray();
        return ids[0] + ":" + ids[1];
    }

    /// <summary>
    /// Создаёт групповой чат. Создатель получает роль Owner.
    /// </summary>
    public Chat CreateGroupChat(string name, Guid creatorId, List<Guid> memberIds)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("Название группы не может быть пустым.");

        var chat = new Chat
        {
            Type = ChatType.Group,
            Name = name.Trim(),
            CreatedByUserId = creatorId
        };
        _storage.AddChat(chat);

        _storage.AddChatMember(new ChatMember
        {
            ChatId = chat.Id,
            UserId = creatorId,
            MemberRole = ChatMemberRole.Owner
        });

        foreach (var memberId in memberIds)
        {
            // Создателя второй раз не добавляем.
            if (memberId == creatorId)
                continue;

            if (_storage.GetUserById(memberId) == null)
                throw new AppException("Один из участников не найден.");

            _storage.AddChatMember(new ChatMember { ChatId = chat.Id, UserId = memberId });
        }

        return chat;
    }

    /// <summary>
    /// Отправляет сообщение, предварительно проверив участие в чате.
    /// </summary>
    public Message SendMessage(Guid chatId, Guid senderId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new AppException("Текст сообщения не может быть пустым.");

        if (_storage.GetChatById(chatId) == null)
            throw new AppException("Чат не найден.");

        if (!_storage.IsChatMember(chatId, senderId))
            throw new AppException("Вы не состоите в этом чате.");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Text = text.Trim()
        };
        _storage.AddMessage(message);
        return message;
    }

    /// <summary>
    /// История сообщений чата. Доступна только участникам.
    /// </summary>
    public List<Message> GetHistory(Guid chatId, Guid userId)
    {
        if (!_storage.IsChatMember(chatId, userId))
            throw new AppException("Вы не состоите в этом чате.");

        return _storage.GetMessagesOfChat(chatId);
    }

    /// <summary>
    /// Все чаты, в которых состоит пользователь.
    /// </summary>
    public List<Chat> GetChatsOfUser(Guid userId)
    {
        var chats = new List<Chat>();

        foreach (var membership in _storage.GetMembershipsOfUser(userId))
        {
            var chat = _storage.GetChatById(membership.ChatId);
            if (chat != null)
                chats.Add(chat);
        }

        return chats;
    }

    /// <summary>
    /// Заголовок чата для показа в списке:
    /// для группы это её название, для личного чата это ФИО собеседника.
    /// </summary>
    public string GetChatTitle(Guid chatId, Guid forUserId)
    {
        var chat = _storage.GetChatById(chatId) ?? throw new AppException("Чат не найден.");

        if (chat.Type == ChatType.Group)
            return chat.Name ?? "Группа";

        foreach (var member in _storage.GetMembersOfChat(chatId))
        {
            if (member.UserId != forUserId)
            {
                var other = _storage.GetUserById(member.UserId);
                if (other != null)
                    return other.FullName;
            }
        }

        return "Личный чат";
    }
}
