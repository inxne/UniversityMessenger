namespace UniversityMessenger.Core.Models;

/// <summary>
/// Сообщение в чате. Хранится только в зашифрованном виде.
/// </summary>
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }

    /// <summary>
    /// Шифротекст: base64 от склейки nonce + tag + зашифрованные байты.
    /// Открытого текста на сервере нет вообще.
    /// </summary>
    public string Ciphertext { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
