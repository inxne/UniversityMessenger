# Запуск проекта на новом ПК

Пошаговая инструкция: от чистой Windows до работающего сервера.

## 1. Установить программы

### Git
https://git-scm.com/download/win
Ставить с настройками по умолчанию. В комплекте идёт Git Credential Manager:
он сам откроет браузер для входа в GitHub при первом push.

### .NET SDK 10
https://dotnet.microsoft.com/download/dotnet/10.0
Выбирать SDK x64. После установки проверить в терминале:

    dotnet --version

### Visual Studio 2022 Community (необязательно)
https://visualstudio.microsoft.com/ru/downloads/
При установке отметить рабочую нагрузку "ASP.NET и веб-разработка".
Проект полностью собирается и запускается из терминала без Visual Studio.

### DB Browser for SQLite (необязательно)
https://sqlitebrowser.org
Для просмотра таблиц файла messenger.db глазами.

## 2. Скачать код

    git clone https://github.com/ЗАМЕНИ_НА_СВОЙ_ЛОГИН/UniversityMessenger.git
    cd UniversityMessenger

## 3. Собрать

    dotnet build

Зелёными должны быть три проекта: Core, Console, Server.

## 4. Демо ядра в консоли

    dotnet run --project src/UniversityMessenger.Console

Покажет сценарий: регистрация, вход, поиск, личный чат, группа, история.

## 5. Запуск сервера

    dotnet run --project src/UniversityMessenger.Server

Адреса:
- Swagger: http://localhost:5000/swagger
- Страница проверки реалтайма: http://localhost:5000/index.html
- Проверка здоровья: http://localhost:5000/api/health

При первом старте рядом с сервером создаётся файл базы messenger.db.

## 6. Создать первых пользователей

Через Swagger: POST /api/auth/register.
Или из PowerShell одной командой:

    Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/register -ContentType 'application/json; charset=utf-8' -Body '{"email":"student@test.local","password":"Student123!","fullName":"Иванов Иван Иванович","role":0,"faculty":"Информационные технологии","course":2,"consent":true}'

Токен для закрытых эндпоинтов: POST /api/auth/login, поле token в ответе.

## 7. Сброс тестовых данных

Остановить сервер (Ctrl+C) и удалить файл базы:

    Remove-Item src\UniversityMessenger.Server\messenger.db

При следующем старте сервер создаст пустую базу.

## 8. Если планируется сборка клиента MAUI

    dotnet workload install maui

Либо в Visual Studio рабочая нагрузка ".NET Multi-platform App UI (MAUI)".

## Частые проблемы

- Порт 5000 занят: закрыта не вся прошлая копия сервера. Найти её терминал и нажать Ctrl+C.
- Swagger не открывается: сервер не запущен. В его терминале должна быть строка Now listening on: http://localhost:5000.
- Clone просит логин и пароль: ввести логин GitHub; Git Credential Manager откроет браузер для входа.
- Ошибка dotnet не является командой: не установлен SDK или терминал открыт до установки. Открыть терминал заново.
