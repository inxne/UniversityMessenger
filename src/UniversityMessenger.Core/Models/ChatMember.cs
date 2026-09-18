namespace UniversityMessenger.Core.Models;

/// <summary>
/// Роль участника внутри чата.
/// </summary>
public enum ChatMemberRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}

/// <summary>
/// Связь "чат - пользователь": кто состоит в каком чате.
/// </summary>
public class ChatMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public ChatMemberRole MemberRole { get; set; } = ChatMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
