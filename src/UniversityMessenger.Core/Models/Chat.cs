namespace UniversityMessenger.Core.Models;

/// <summary>
/// Чат: личный или групповой.
/// </summary>
public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChatType Type { get; set; } = ChatType.Direct;
    public string? Name { get; set; }
    public string? DirectKey { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
