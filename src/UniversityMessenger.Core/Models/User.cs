namespace UniversityMessenger.Core.Models;

/// <summary>
/// Пользователь системы: студент, преподаватель или администратор.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Почта пользователя. Используется для входа.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля. Пароль в открытом виде хранить нельзя.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Полное имя пользователя. Например: Иванов Иван Иванович.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Полное имя в нижнем регистре. Нужно для поиска без учёта регистра.
    /// </summary>
    public string FullNameLower { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public Role Role { get; set; } = Role.Student;

    /// <summary>
    /// Факультет. Может быть пустым.
    /// </summary>
    public string? Faculty { get; set; }

    /// <summary>
    /// Курс. Может быть пустым.
    /// </summary>
    public int? Course { get; set; }

    /// <summary>
    /// Информация о пользователе: группа, должность, интересы.
    /// </summary>
    public string? About { get; set; }

    /// <summary>
    /// Подтверждён ли пользователь администратором.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Активен ли аккаунт. Если заблокирован — false.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата и время создания аккаунта.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата согласия на обработку персональных данных.
    /// </summary>
    public DateTime? ConsentAt { get; set; }
}