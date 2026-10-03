namespace UniversityMessenger.Core.Models;

/// <summary>
/// Обёрнутый ключ группового чата для конкретного участника.
/// Открыть конверт может только адресат: он зашифрован парным
/// ECDH-секретом создателя группы и этого участника.
/// Сервер хранит конверты, но не может открыть ни один.
/// </summary>
public class ChatKeyWrap
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatId { get; set; }
    public Guid ForUserId { get; set; }
    public string WrappedKey { get; set; } = string.Empty;
}
