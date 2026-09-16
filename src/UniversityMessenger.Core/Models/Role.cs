namespace UniversityMessenger.Core.Models;

/// <summary>
/// Роли пользователей в системе.
/// </summary>
public enum Role
{
    /// <summary>
    /// Студент.
    /// </summary>
    Student = 0,

    /// <summary>
    /// Преподаватель.
    /// </summary>
    Teacher = 1,

    /// <summary>
    /// Администратор.
    /// </summary>
    Admin = 2
}