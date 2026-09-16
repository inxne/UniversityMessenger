namespace UniversityMessenger.Core.Models;

/// <summary>
/// Тип чата.
/// </summary>
public enum ChatType
{
    /// <summary>
    /// Личный чат между двумя пользователями.
    /// </summary>
    Direct = 0,

    /// <summary>
    /// Групповой чат.
    /// </summary>
    Group = 1
}