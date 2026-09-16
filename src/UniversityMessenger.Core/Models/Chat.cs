namespace UniversityMessenger.Core.Models;

/// <summary>
/// Чат: личный или групповой.
/// </summary>
public class Chat
{
    /// <summary>
    /// Уникальный идентификатор чата.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Тип чата: Direct или Group.
    /// </summary>
    public ChatType Type { get; set; } = ChatType.Direct;

    /// <summary>
    /// Название группы. Для личного чата остаётся пустым.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Ключ личного чата.
    /// Состоит из двух Id пользователей, отсортированных по возрастанию.
    /// Нужен, чтобы один и тот же личный чат не создавался дважды.
    /// </summary>
    public string? DirectKey { get; set; }

    /// <summary>
    /// Кто создал чат. Для личного чата может быть пустым.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Дата и время создания чата.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}