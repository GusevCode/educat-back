# VibeTech Educat - Платформа для репетиторов и студентов

## Описание
VibeTech Educat - это веб-платформа для организации онлайн-обучения, которая связывает репетиторов и студентов. Платформа предоставляет возможности для:
- Поиска и выбора репетиторов по различным предметам
- Управления расписанием уроков
- Проведения онлайн-занятий с видеоконференциями и виртуальной доской
- Обмена учебными материалами
- Отслеживания прогресса обучения

## Требования
- .NET 8.0 SDK
- PostgreSQL 14+
- JetBrains Rider или Visual Studio 2022

## Настройка проекта

### 1. База данных
1. Установите PostgreSQL
2. Создайте базу данных `EducatDb`
3. Обновите строку подключения в `src/Vibetech.Educat.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EducatDb;Username=youruser;Password=yourpassword"
}
```

### 2. Конфигурация безопасности
1. Создайте файл `src/Vibetech.Educat.Web/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EducatDb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-super-secret-key-minimum-256-bits-long-for-security"
  }
}
```

2. Для production используйте переменные окружения или Azure Key Vault

### 3. Применение миграций
```bash
cd src
dotnet ef database update -p Vibetech.Educat.DataAccess -s Vibetech.Educat.Web
```

### 4. Запуск проекта

#### Через Rider:
1. Откройте файл решения `src/Vibetech.Educat.sln`
2. Выберите конфигурацию запуска `Vibetech.Educat.Web`
3. Нажмите Run (Shift+F10)

#### Через командную строку:
```bash
cd src/Vibetech.Educat.Web
dotnet run
```

Приложение будет доступно по адресу: https://localhost:7126 или http://localhost:5125

## API документация
После запуска проекта Swagger UI доступен по адресу: http://localhost:5125/swagger

## Структура проекта
- `Vibetech.Educat.Web` - Основной веб-проект с конфигурацией
- `Vibetech.Educat.API` - Контроллеры и модели API
- `Vibetech.Educat.Services` - Бизнес-логика
- `Vibetech.Educat.DataAccess` - Доступ к данным, Entity Framework

## Роли пользователей
- `STUDENT` - Студент, может искать репетиторов и записываться на уроки
- `TEACHER` - Репетитор, может создавать уроки и управлять студентами

## Безопасность
- JWT аутентификация для API
- Авторизация на основе ролей
- Пароли хешируются с помощью BCrypt

## Известные проблемы
- CORS настроен на AllowAll для разработки - измените для production
- JWT ключ должен быть изменен и храниться безопасно 