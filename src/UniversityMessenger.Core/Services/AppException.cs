namespace UniversityMessenger.Core.Services;

/// <summary>
/// Ошибка бизнес-логики с понятным сообщением на русском.
/// Сервисы бросают её, когда пользователь сделал что-то недопустимое.
/// </summary>
public class AppException : Exception
{
    public AppException(string message) : base(message)
    {
    }
}
