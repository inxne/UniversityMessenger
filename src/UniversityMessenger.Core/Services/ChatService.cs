using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Storage;

namespace UniversityMessenger.Core.Services;

/// <summary>
/// Чаты, участники и сообщения.
/// Содержимое сообщений и ключи для сервиса непрозрачны.
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

    private static string MakeDirectKey(Guid a, Guid b)
    {
        var ids = new[] { a, b }.OrderBy(x => x).ToArray();
        return ids[0] + ":" + ids[1];
    }

    /// <summary>
    /// Создаёт групповой чат. Создатель получает роль Owner.
    /// keyWraps это конверты ключа группы: по одному на каждого участника
    /// включая самого создателя, иначе при повторном входе он не откроет чат.
    /// Сервис не видит содержимое конвертов, только полноту набора.
    /// </summary>
    public Chat CreateGroupChat(string name, Guid creatorId, List<Guid> memberIds, List<ChatKeyWrap> keyWraps)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("Название группы не может быть пустым.");

        var expected = memberIds.Where(id => id != creatorId).Append(creatorId).Distinct().ToList();

        foreach (var userId in expected)
        {
            if (keyWraps.Count(w => w.ForUserId == userId) != 1)
                throw new AppException("Не каждый участник обеспечен обёрнутым ключом группы.");
        }

        var chat = new Chat
        {
            Type = ChatType.Group,
            Name = name.Trim(),
            CreatedByUserId = creatorId
        };

        foreach (var wrap in keyWraps)
        {
            wrap.ChatId = chat.Id;
            chat.KeyWraps.Add(wrap);
        }

        _storage.AddChat(chat);

        _storage.AddChatMember(new ChatMember
        {
            ChatId = chat.Id,
            UserId = creatorId,
            MemberRole = ChatMemberRole.Owner
        });

        foreach (var memberId in memberIds)
        {
            if (memberId == creatorId)
                continue;

            if (_storage.GetUserById(memberId) == null)
                throw new AppException("Один из участников не найден.");

            _storage.AddChatMember(new ChatMember { ChatId = chat.Id, UserId = memberId });
        }

        return chat;
    }

    /// <summary>
    /// Отправляет сообщение. Принимает шифротекст как непрозрачную строку.
    /// </summary>
    public Message SendMessage(Guid chatId, Guid senderId, string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
            throw new AppException("Шифротекст сообщения не может быть пустым.");

        if (_storage.GetChatById(chatId) == null)
            throw new AppException("Чат не найден.");

        if (!_storage.IsChatMember(chatId, senderId))
            throw new AppException("Вы не состоите в этом чате.");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Ciphertext = ciphertext
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
    /// Конверт ключа группы для конкретного участника.
    /// Участник открывает его своим парным секретом с создателем.
    /// </summary>
    public List<ChatKeyWrap> GetKeyWrapsForUser(Guid chatId, Guid userId)
    {
        if (!_storage.IsChatMember(chatId, userId))
            throw new AppException("Вы не состоите в этом чате.");

        var chat = _storage.GetChatById(chatId) ?? throw new AppException("Чат не найден.");

        return chat.KeyWraps.Where(w => w.ForUserId == userId).ToList();
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
    /// Заголовок чата для показа в списке.
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
