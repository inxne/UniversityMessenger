using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Storage;

namespace UniversityMessenger.Core.Services;

/// <summary>
/// Поиск пользователей и выдача списков.
/// </summary>
public class UserService
{
    private readonly IStorage _storage;

    public UserService(IStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Ищет активных пользователей.
    /// Все параметры необязательные: что не задано, то не проверяется.
    /// </summary>
    public List<User> Search(string? query = null, Role? role = null, string? faculty = null, int? course = null)
    {
        var result = new List<User>();

        foreach (var user in _storage.GetUsers())
        {
            // Заблокированные не попадают в поиск.
            if (!user.IsActive)
                continue;

            // Поиск по ФИО без учёта регистра: ищем по FullNameLower.
            if (query != null && !user.FullNameLower.Contains(query.ToLowerInvariant()))
                continue;

            // Фильтр по роли.
            if (role != null && user.Role != role.Value)
                continue;

            // Фильтр по факультету, тоже без учёта регистра.
            if (faculty != null && (user.Faculty == null || !user.Faculty.ToLowerInvariant().Contains(faculty.ToLowerInvariant())))
                continue;

            // Фильтр по курсу.
            if (course != null && user.Course != course.Value)
                continue;

            result.Add(user);
        }

        return result;
    }
}
