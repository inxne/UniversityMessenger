namespace UniversityMessenger.Core.Models;

/// <summary>
/// Переписка: личная или групповая.
/// </summary>
public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChatType Type { get; set; }
    public string? Name { get; set; }
    public string? DirectKey { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Обёрнутые ключи группы, по одному на участника.
    /// Для личных чатов список пуст: секрет выводится парным ECDH.
    /// </summary>
    public List<ChatKeyWrap> KeyWraps { get; set; } = new();
}
