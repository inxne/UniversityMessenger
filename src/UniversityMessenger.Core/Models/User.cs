namespace UniversityMessenger.Core.Models;

/// <summary>
/// Пользователь системы: сотрудник, руководитель или администратор безопасности.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FullNameLower { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Student;
    public string? Faculty { get; set; }
    public int? Course { get; set; }
    public string? About { get; set; }

    /// <summary>
    /// Публичный ключ шифрования клиента, base64.
    /// Нужен собеседникам для вывода общего секрета чата по ECDH.
    /// Приватный ключ сюда не попадает никогда.
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConsentAt { get; set; }
}
