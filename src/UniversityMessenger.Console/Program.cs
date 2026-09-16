using UniversityMessenger.Core.Models;

// ---------- 1. Создаём двух пользователей ----------

var studentFullName = "Иванов Иван Иванович";
var student = new User
{
    Email = "student@test.local",
    FullName = studentFullName,
    FullNameLower = studentFullName.ToLowerInvariant(),
    Role = Role.Student,
    Faculty = "Информационные технологии",
    Course = 2,
    ConsentAt = DateTime.UtcNow
};

var teacherFullName = "Петрова Анна Сергеевна";
var teacher = new User
{
    Email = "teacher@test.local",
    FullName = teacherFullName,
    FullNameLower = teacherFullName.ToLowerInvariant(),
    Role = Role.Teacher,
    Faculty = "Информационные технологии",
    About = "Преподаватель кафедры ИТ",
    ConsentAt = DateTime.UtcNow
};

// ---------- 2. Создаём личный чат между ними ----------

// Сортируем Id пользователей, чтобы ключ чата всегда был одинаковым
// независимо от того, кто первым нажал кнопку "Написать".
var ids = new[] { student.Id, teacher.Id }.OrderBy(x => x).ToArray();
var directKey = $"{ids[0]}:{ids[1]}";

var chat = new Chat
{
    Type = ChatType.Direct,
    DirectKey = directKey
};

// ---------- 3. Добавляем обоих пользователей в чат ----------

var studentMember = new ChatMember
{
    ChatId = chat.Id,
    UserId = student.Id,
    MemberRole = ChatMemberRole.Member
};

var teacherMember = new ChatMember
{
    ChatId = chat.Id,
    UserId = teacher.Id,
    MemberRole = ChatMemberRole.Member
};

// ---------- 4. Создаём два сообщения ----------

var message1 = new Message
{
    ChatId = chat.Id,
    SenderId = student.Id,
    Text = "Здравствуйте! Можно задать вопрос по проекту?"
};

var message2 = new Message
{
    ChatId = chat.Id,
    SenderId = teacher.Id,
    Text = "Да, конечно. Слушаю."
};

// ---------- 5. Выводим результат в консоль ----------

Console.WriteLine("=== Пользователи ===");
Console.WriteLine($"{student.FullName} ({student.Role})");
Console.WriteLine($"{teacher.FullName} ({teacher.Role})");

Console.WriteLine();
Console.WriteLine("=== Чат ===");
Console.WriteLine($"Тип: {chat.Type}");
Console.WriteLine($"Ключ личного чата: {chat.DirectKey}");

Console.WriteLine();
Console.WriteLine("=== Участники чата ===");
Console.WriteLine($"Участник 1: {studentMember.UserId}");
Console.WriteLine($"Участник 2: {teacherMember.UserId}");

Console.WriteLine();
Console.WriteLine("=== Сообщения ===");
Console.WriteLine($"{student.FullName}: {message1.Text}");
Console.WriteLine($"{teacher.FullName}: {message2.Text}");