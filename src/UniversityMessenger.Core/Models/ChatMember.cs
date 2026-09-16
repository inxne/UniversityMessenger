namespace UniversityMessenger.Core.Models;

/// <summary>
/// Роль участника внутри чата.
/// </summary>
public enum ChatMemberRole
{
	/// <summary>
	/// Обычный участник.
	/// </summary>
	Member = 0,

	/// <summary>
	/// Администратор группы.
	/// </summary>
	Admin = 1,

	/// <summary>
	/// Создатель группы.
	/// </summary>
	Owner = 2
}

/// <summary>
/// Связь "чат — пользователь": кто состоит в каком чате.
/// </summary>
public class ChatMember
{
	/// <summary>
	/// Уникальный идентификатор записи.
	/// </summary>
	public Guid Id { get; set; } = Guid.NewGuid();

	/// <summary>
	/// В какой чат входит пользователь.
	/// </summary>
	public Guid ChatId { get; set; }

	/// <summary>
	/// Какой пользователь входит в чат.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Роль пользователя внутри этого чата.
	/// </summary>
	public ChatMemberRole MemberRole { get; set; } = ChatMemberRole.Member;

	/// <summary>
	/// Дата и время вступления в чат.
	/// </summary>
	public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}