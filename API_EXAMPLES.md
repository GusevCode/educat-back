# Примеры использования API VibeTech Educat

## Базовый URL
```
Development: http://localhost:5125
Production: https://yourdomain.com
```

## Аутентификация

### Регистрация студента
```http
POST /api/auth/register/student
Content-Type: application/json

{
  "login": "student@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "Иван",
  "lastName": "Иванов",
  "middleName": "Иванович",
  "birthDate": "2000-01-01",
  "gender": "М",
  "contactInfo": "+7 999 123-45-67"
}
```

### Регистрация репетитора
```http
POST /api/auth/register/teacher
Content-Type: application/json

{
  "login": "teacher@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "Петр",
  "lastName": "Петров",
  "middleName": "Петрович",
  "birthDate": "1985-05-15",
  "gender": "М",
  "contactInfo": "+7 999 987-65-43",
  "education": "МГУ, математический факультет",
  "experienceYears": 10,
  "hourlyRate": 1500,
  "subjectIds": [1, 2]
}
```

### Вход в систему
```http
POST /api/auth/login
Content-Type: application/json

{
  "login": "student@example.com",
  "password": "Password123!"
}
```

Ответ:
```json
{
  "success": true,
  "message": "Вход выполнен успешно",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "email": "student@example.com",
    "firstName": "Иван",
    "lastName": "Иванов",
    "roles": ["STUDENT"]
  }
}
```

## Работа с профилем

### Получение профиля репетитора
```http
GET /api/teacher/profile/1
Authorization: Bearer {token}
```

### Обновление профиля репетитора
```http
PUT /api/teacher/profile/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Петр",
  "lastName": "Петров",
  "education": "МГУ, математический факультет, кандидат наук",
  "hourlyRate": 2000,
  "subjectIds": [1, 2, 3]
}
```

## Поиск репетиторов

### Поиск по критериям
```http
GET /api/student/search-tutors?subjectId=1&minPrice=1000&maxPrice=3000&minRating=4.5
Authorization: Bearer {token}
```

## Управление уроками

### Создание урока (для репетитора)
```http
POST /api/teacher/create-lesson
Authorization: Bearer {token}
Content-Type: application/json

{
  "teacherId": 1,
  "studentId": 2,
  "subjectId": 1,
  "startTime": "2025-06-01T10:00:00",
  "endTime": "2025-06-01T11:00:00",
  "conferenceLink": "https://zoom.us/j/123456789",
  "whiteboardLink": "https://whiteboard.app/session/abc123"
}
```

### Получение уроков студента
```http
GET /api/student/1/lessons?startDate=2025-06-01&endDate=2025-06-30
Authorization: Bearer {token}
```

## Заявки

### Отправка заявки репетитору
```http
POST /api/student/send-request/1?studentId=2
Authorization: Bearer {token}
```

### Принятие заявки репетитором
```http
POST /api/teacher/accept-request/1
Authorization: Bearer {token}
```

## Справочники

### Получение списка предметов
```http
GET /api/dictionary/subjects
```

### Получение программ подготовки
```http
GET /api/dictionary/preparation-programs
```

## Работа с файлами

### Загрузка файла к уроку
```http
POST /api/teacher/lesson/1/upload-attachment?teacherId=1
Authorization: Bearer {token}
Content-Type: application/json

{
  "fileName": "homework.pdf",
  "fileType": "application/pdf",
  "base64Content": "JVBERi0xLjQKJcOkw7zDtsOfCjI..."
}
```

### Получение вложений урока
```http
GET /api/student/lesson/1/attachments
Authorization: Bearer {token}
```

## Статистика

### Статистика репетитора
```http
GET /api/teacher/1/statistics
Authorization: Bearer {token}
```

Ответ:
```json
{
  "totalStudents": 15,
  "totalLessons": 120,
  "completedLessons": 100,
  "upcomingLessons": 20,
  "rating": 4.8,
  "reviewsCount": 45,
  "lessonsBySubject": {
    "1": 60,
    "2": 40,
    "3": 20
  }
}
```

## Коды ошибок

- `200 OK` - Успешное выполнение
- `400 Bad Request` - Некорректный запрос
- `401 Unauthorized` - Требуется аутентификация
- `403 Forbidden` - Доступ запрещен
- `404 Not Found` - Ресурс не найден
- `500 Internal Server Error` - Внутренняя ошибка сервера 