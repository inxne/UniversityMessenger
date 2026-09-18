using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Storage;

namespace UniversityMessenger.Core.Services;

/// <summary>
/// Регистрация и вход пользователей.
/// </summary>
public class AuthService
{
    // Хранилище сервис получает извне через конструктор.
    // Это внедрение зависимости: сервису всё равно,
    // память это или база данных.
    private readonly IStorage _storage;

    public AuthService(IStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Регистрирует нового пользователя и возвращает его.
    /// </summary>
    public User Register(string email, string password, string fullName, Role role, string? faculty, int? course, bool consent)
    {
        if (!consent)
            throw new AppException("Регистрация невозможна без согласия на обработку персональных данных.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new AppException("Некорректная почта.");

        if (password.Length < 8)
            throw new AppException("Пароль должен быть не короче 8 символов.");

        if (fullName.Trim().Length < 5)
            throw new AppException("ФИО должно быть не короче 5 символов.");

        if (_storage.GetUserByEmail(email) != null)
            throw new AppException("Пользователь с такой почтой уже зарегистрирован.");

        var user = new User
        {
            Email = email,
            // В хранилище кладём НЕ пароль, а его хеш. Обратно он не превращается.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName.Trim(),
            // Дубль ФИО в нижнем регистре для поиска без учёта регистра.
            FullNameLower = fullName.Trim().ToLowerInvariant(),
            Role = role,
            Faculty = faculty,
            Course = course,
            ConsentAt = DateTime.UtcNow
        };

        _storage.AddUser(user);
        return user;
    }

    /// <summary>
    /// Вход по почте и паролю. Возвращает пользователя, если всё верно.
    /// </summary>
    public User Login(string email, string password)
    {
        var user = _storage.GetUserByEmail(email);

        // Сверяем не пароли, а хеш введённого пароля с сохранённым хешем.
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new AppException("Неверная почта или пароль.");

        if (!user.IsActive)
            throw new AppException("Аккаунт заблокирован администратором.");

        return user;
    }

    /// <summary>
    /// Возвращает пользователя по Id или ошибку, если его нет.
    /// </summary>
    public User GetById(Guid id)
    {
        return _storage.GetUserById(id) ?? throw new AppException("Пользователь не найден.");
    }
}
