using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UniversityMessenger.Core.Services;

namespace UniversityMessenger.Server.Hubs;

/// <summary>
/// SignalR-хаб: реалтайм-доставка сообщений участникам чатов.
/// Подключаться могут только авторизованные пользователи: атрибут Authorize.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly ChatService _chats;
    private readonly AuthService _auth;

    public ChatHub(ChatService chats, AuthService auth)
    {
        _chats = chats;
        _auth = auth;
    }

    /// <summary>
    /// Подписка на чат. Разрешена только участникам чата.
    /// </summary>
    public async Task JoinChat(Guid chatId)
    {
        var userId = Context.User!.UserId();

        var myChats = _chats.GetChatsOfUser(userId);
        if (!myChats.Any(c => c.Id == chatId))
        {
            await Clients.Caller.SendAsync("Error", "Вы не состоите в этом чате.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        await Clients.Caller.SendAsync("Info", "Подписка на чат оформлена.");
    }

    /// <summary>
    /// Отправка сообщения: сохраняем через сервис и сразу
    /// рассылаем всей группе чата.
    /// </summary>
    public async Task SendMessage(Guid chatId, string text)
    {
        var userId = Context.User!.UserId();

        try
        {
            var message = _chats.SendMessage(chatId, userId, text);
            var sender = _auth.GetById(userId);
            var dto = new MessageDto(message, sender.FullName);
            await Clients.Group(chatId.ToString()).SendAsync("MessageReceived", dto);
        }
        catch (AppException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }
}
