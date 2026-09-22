# University Messenger

Внутривузовский мессенджер для студентов, преподавателей и администраторов.
Учебный проект 2 курса.

## Стек

- C#, .NET 10
- ASP.NET Core (сервер, минимальные API)
- JWT-токены через Microsoft.AspNetCore.Authentication.JwtBearer
- .NET MAUI (клиент, в разработке)
- BCrypt.Net-Next (хеширование паролей)
- Swashbuckle (Swagger)

## Структура

- src/UniversityMessenger.Core — ядро: модели, хранилище, сервисы
- src/UniversityMessenger.Console — консольное демо логики ядра
- src/UniversityMessenger.Server — сервер API, http://localhost:5000, Swagger на /swagger

## Документация репозитория

- docs/architecture.md — карта всех файлов и их назначения
- docs/changelog.md — журнал изменений на русском по каждому коммиту
- docs/defense-notes.md — шпаргалки к защите, вопросы и ответы

## Возможности ядра

- Регистрация с проверками и согласием на обработку персональных данных
- Вход по почте и паролю, пароль хранится только в виде хеша
- Вход выдаёт JWT-токен, закрытые эндпоинты знают пользователя по токену
- Поиск пользователей по ФИО без учёта регистра, фильтры по роли, факультету, курсу
- Личные чаты (один чат на пару людей через DirectKey) и групповые чаты
- Отправка сообщений с проверкой участия в чате, история переписки

## Запуск демо ядра

    dotnet run --project src/UniversityMessenger.Console

## Запуск сервера

    dotnet run --project src/UniversityMessenger.Server

Затем открыть http://localhost:5000/swagger

## Статус

- [x] Ядро: модели, хранилище, сервисы, консольное демо
- [x] Сервер: каркас, health-эндпоинт, Swagger
- [x] Эндпоинты auth, users, chats, messages
- [x] Аутентификация через JWT-токены
- [ ] База данных SQLite через EF Core
- [ ] Реалтайм через SignalR
- [ ] Клиент .NET MAUI
