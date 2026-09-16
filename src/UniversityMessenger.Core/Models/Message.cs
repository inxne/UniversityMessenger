namespace UniversityMessenger.Core.Models;

/// <summary>
/// Сообщение в чате.
/// </summary>
public class Message
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// В каком чате написано сообщение.
    /// </summary>
    public Guid ChatId { get; set; }

    /// <summary>
    /// Кто отправил сообщение.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время отправки.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата удаления сообщения. Если сообщение не удалено — пусто.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}