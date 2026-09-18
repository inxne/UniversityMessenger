using UniversityMessenger.Core.Models;
using UniversityMessenger.Core.Services;
using UniversityMessenger.Core.Storage;

// --- Сборка приложения: хранилище плюс сервисы ---
IStorage storage = new InMemoryStorage();
var authService = new AuthService(storage);
var userService = new UserService(storage);
var chatService = new ChatService(storage);

// --- 1. Регистрируем трёх пользователей ---
var student = authService.Register(
    email: "student@test.local",
    password: "Student123!",
    fullName: "Иванов Иван Иванович",
    role: Role.Student,
    faculty: "Информационные технологии",
    course: 2,
    consent: true);

var teacher = authService.Register(
    email: "teacher@test.local",
    password: "Teacher123!",
    fullName: "Петрова Анна Сергеевна",
    role: Role.Teacher,
    faculty: "Информационные технологии",
    course: null,
    consent: true);

var admin = authService.Register(
    email: "admin@test.local",
    password: "Admin12345!",
    fullName: "Сидоров Алексей Дмитриевич",
    role: Role.Admin,
    faculty: null,
    course: null,
    consent: true);

Console.WriteLine("Зарегистрированы:");
Console.WriteLine($"- {student.FullName} ({student.Role})");
Console.WriteLine($"- {teacher.FullName} ({teacher.Role})");
Console.WriteLine($"- {admin.FullName} ({admin.Role})");

// --- 2. Вход под студентом ---
var logged = authService.Login("student@test.local", "Student123!");
Console.WriteLine();
Console.WriteLine($"Вход выполнен: {logged.FullName}");

// --- 2.1. Проверка защиты: неверный пароль ---
try
{
    authService.Login("student@test.local", "wrong-password");
}
catch (AppException ex)
{
    Console.WriteLine($"Ожидаемая ошибка входа: {ex.Message}");
}

// --- 3. Поиск пользователя по фамилии ---
var found = userService.Search(query: "петрова");
Console.WriteLine();
Console.WriteLine($"Поиск по запросу петрова: найдено {found.Count}");
foreach (var u in found)
{
    Console.WriteLine($"- {u.FullName}, {u.Role}, {u.Faculty}");
}

// --- 4. Личный чат и переписка ---
var directChat = chatService.GetOrCreateDirectChat(student.Id, teacher.Id);
var directAgain = chatService.GetOrCreateDirectChat(teacher.Id, student.Id);
Console.WriteLine();
Console.WriteLine($"Личный чат создан. Повторный вызов вернул тот же чат: {directAgain.Id == directChat.Id}");

chatService.SendMessage(directChat.Id, student.Id, "Здравствуйте! Можно задать вопрос по проекту?");
chatService.SendMessage(directChat.Id, teacher.Id, "Да, конечно. Слушаю.");
chatService.SendMessage(directChat.Id, student.Id, "Как оформить диаграмму архитектуры?");

// --- 5. Групповой чат ---
var groupChat = chatService.CreateGroupChat("ИТ-201: Проектная деятельность", teacher.Id, new List<Guid> { student.Id, admin.Id });
chatService.SendMessage(groupChat.Id, teacher.Id, "Коллеги, защита проекта в пятницу.");

// --- 6. История сообщений ---
Console.WriteLine();
Console.WriteLine($"История личного чата ({chatService.GetChatTitle(directChat.Id, student.Id)}):");
foreach (var message in chatService.GetHistory(directChat.Id, student.Id))
{
    var author = authService.GetById(message.SenderId);
    Console.WriteLine($"[{message.CreatedAt:HH:mm}] {author.FullName}: {message.Text}");
}

Console.WriteLine();
Console.WriteLine($"История группы ({chatService.GetChatTitle(groupChat.Id, student.Id)}):");
foreach (var message in chatService.GetHistory(groupChat.Id, student.Id))
{
    var author = authService.GetById(message.SenderId);
    Console.WriteLine($"[{message.CreatedAt:HH:mm}] {author.FullName}: {message.Text}");
}

// --- 7. Список чатов студента ---
Console.WriteLine();
Console.WriteLine("Чаты студента:");
foreach (var chat in chatService.GetChatsOfUser(student.Id))
{
    var title = chatService.GetChatTitle(chat.Id, student.Id);
    var last = chatService.GetHistory(chat.Id, student.Id).LastOrDefault();
    var lastText = last == null ? "нет сообщений" : last.Text;
    Console.WriteLine($"- {title}: {lastText}");
}
