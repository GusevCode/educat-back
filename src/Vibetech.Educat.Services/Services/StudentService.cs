using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Vibetech.Educat.Services.DTO;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Vibetech.Educat.Services.Services;

public class StudentService : BaseService
{
    private readonly ILogger<StudentService> _studentLogger;

    public StudentService(EducatDbContext context, ILogger<StudentService> logger)
        : base(context, logger)
    {
        _studentLogger = logger;
    }

    public async Task<StudentProfile> GetStudentProfileAsync(int studentId)
    {
        _studentLogger.LogInformation("Поиск профиля студента с ID: {StudentId}", studentId);
        
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
        {
            _studentLogger.LogWarning("Студент с ID: {StudentId} не найден", studentId);
            return null!;
        }
            
        _studentLogger.LogInformation("Профиль студента найден для ID: {StudentId}", studentId);
        
        return new StudentProfile
        {
            UserId = student.Id,
            FullName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
            BirthDate = student.BirthDate,
            Gender = student.Gender,
            ContactInfo = student.ContactInformation,
            PhotoBase64 = student.PhotoBase64,
            Email = student.Email,
            FirstName = student.FirstName,
            LastName = student.LastName,
            MiddleName = student.MiddleName ?? string.Empty
        };
    }

    public async Task<StudentProfile> UpdateStudentProfileAsync(
        int studentId, 
        string firstName, 
        string lastName, 
        string middleName, 
        DateTime birthDate, 
        string gender, 
        string contactInfo,
        string photoBase64 = null)
    {
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
            return null!;
            
        student.FirstName = firstName;
        student.LastName = lastName;
        student.MiddleName = middleName;
        student.BirthDate = birthDate;
        student.Gender = gender;
        student.ContactInformation = contactInfo;
        
        if (!string.IsNullOrEmpty(photoBase64))
        {
            // Обновляем фото пользователя
            student.PhotoBase64 = photoBase64;
        }
        
        await _context.SaveChangesAsync();
        
        return new StudentProfile
        {
            UserId = student.Id,
            FullName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
            BirthDate = student.BirthDate,
            Gender = student.Gender,
            ContactInfo = student.ContactInformation,
            PhotoBase64 = student.PhotoBase64,
            Email = student.Email
        };
    }

    public async Task<IEnumerable<int>> GetStudentTeachersAsync(int studentId)
    {
        _studentLogger.LogInformation("Получение репетиторов для студента с ID: {StudentId}", studentId);

        // Находим всех репетиторов студента и их профили
        var teacherProfiles = await _context.TeacherStudents
            .Where(ts => ts.StudentId == studentId && ts.Status == RequestStatus.Accepted)
            .Join(_context.TeacherProfiles,
                ts => ts.TeacherId,
                tp => tp.Id,
                (ts, tp) => new { TeacherProfile = tp })
            .ToListAsync();

        _studentLogger.LogInformation("Найдено {Count} профилей репетиторов для студента {StudentId}", 
            teacherProfiles.Count, studentId);

        // Возвращаем UserId репетиторов вместо Id
        var teacherUserIds = teacherProfiles.Select(tp => tp.TeacherProfile.UserId).ToList();
        
        _studentLogger.LogInformation("Возвращаем {Count} идентификаторов пользователей-репетиторов", teacherUserIds.Count);
        foreach (var userId in teacherUserIds)
        {
            _studentLogger.LogInformation("UserId репетитора: {UserId}", userId);
        }
        
        return teacherUserIds;
    }

    public async Task<IEnumerable<LessonDTO>> GetStudentLessonsAsync(
        int studentId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        _studentLogger.LogInformation("Получение уроков для студента с ID: {StudentId}", studentId);
        
        var query = _context.Lessons
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.StudentId == studentId);

        if (startDate.HasValue)
        {
            _studentLogger.LogInformation("Фильтрация по начальной дате: {StartDate}", startDate.Value);
            // Предполагаем, что даты приходят в локальном времени, конвертируем их в UTC для правильного сравнения
            DateTime startDateUtc = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.StartTime >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            _studentLogger.LogInformation("Фильтрация по конечной дате: {EndDate}", endDate.Value);
            // Предполагаем, что даты приходят в локальном времени, конвертируем их в UTC для правильного сравнения
            DateTime endDateUtc = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.StartTime <= endDateUtc);
        }
        
        var lessons = await query.ToListAsync();
        
        // Автоматически обновляем статусы уроков через базовый класс
        lessons = (await UpdateLessonsStatusAsync(lessons)).ToList();

        _studentLogger.LogInformation("Найдено {Count} уроков для студента", lessons.Count);
        
        // Сортируем по времени начала
        lessons = lessons.OrderBy(l => l.StartTime).ToList();
        
        // Возвращаем данные с явным указанием формата времени
        return lessons.Select(l => new LessonDTO
        {
            Id = l.Id,
            TeacherId = l.TeacherId,
            StudentId = l.StudentId.Value,
            SubjectId = l.SubjectId,
            TeacherName = $"{l.Teacher.LastName} {l.Teacher.FirstName}".Trim(),
            StudentName = $"{l.Student.LastName} {l.Student.FirstName}".Trim(),
            SubjectName = l.Subject.Name,
            StartTime = l.StartTime, // Не меняем формат времени
            EndTime = l.EndTime,     // Не меняем формат времени
            Status = l.ActualStatusString,
            ConferenceLink = l.ConferenceLink ?? string.Empty,
            BoardLink = l.WhiteboardLink ?? string.Empty
        });
    }

    public async Task<StudentStatisticsDTO> GetStudentStatisticsAsync(int studentId)
    {
        // Получаем все уроки студента
        var lessons = await _context.Lessons
            .Include(l => l.Subject)
            .Where(l => l.StudentId == studentId)
            .ToListAsync();
            
        // Получаем всех репетиторов студента
        var teachersCount = await _context.TeacherStudents
            .Where(ts => ts.StudentId == studentId)
            .CountAsync();
            
        // Подсчет статистики
        var totalLessons = lessons.Count;
        var completedLessons = lessons.Count(l => l.Status == LessonStatus.Completed);
        var upcomingLessons = lessons.Count(l => l.Status == LessonStatus.Scheduled && l.StartTime > DateTime.UtcNow);
        
        // Рассчитываем суммарное время уроков в часах
        var totalHours = lessons
            .Where(l => l.Status == LessonStatus.Completed)
            .Sum(l => (l.EndTime - l.StartTime).TotalHours);
            
        // Распределение уроков по предметам
        var lessonsBySubject = lessons
            .GroupBy(l => l.SubjectId)
            .ToDictionary(g => g.Key, g => g.Count());
            
        return new StudentStatisticsDTO
        {
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            UpcomingLessons = upcomingLessons,
            TeachersCount = teachersCount,
            LessonsBySubject = lessonsBySubject,
            TotalLessonHours = (int)Math.Round(totalHours)
        };
    }

    public async Task<IEnumerable<TeacherProfileDTO>> SearchTutorsAsync(
        int? subjectId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minExperience = null,
        double? minRating = null)
    {
        var query = _context.TeacherProfiles
            .Include(tp => tp.User)
            .Include(tp => tp.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .Include(tp => tp.PreparationPrograms)
            .AsQueryable();

        // Фильтрация по предмету
        if (subjectId.HasValue)
        {
            query = query.Where(tp => tp.TeacherSubjects.Any(ts => ts.SubjectId == subjectId.Value));
        }

        // Фильтрация по цене
        if (minPrice.HasValue)
        {
            query = query.Where(tp => tp.HourlyRate >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(tp => tp.HourlyRate <= maxPrice.Value);
        }

        // Фильтрация по опыту
        if (minExperience.HasValue)
        {
            query = query.Where(tp => tp.ExperienceYears >= minExperience.Value);
        }

        // Фильтрация по рейтингу
        if (minRating.HasValue)
        {
            query = query.Where(tp => tp.Rating >= minRating.Value);
        }

        var teachers = await query.ToListAsync();

        return teachers.Select(t => new TeacherProfileDTO
        {
            UserId = t.UserId,
            FullName = $"{t.User.LastName} {t.User.FirstName} {t.User.MiddleName}".Trim(),
            Education = t.Education,
            ExperienceYears = t.ExperienceYears,
            HourlyRate = t.HourlyRate,
            Rating = t.Rating,
            ReviewsCount = t.ReviewsCount,
            PhotoBase64 = t.User.PhotoBase64,
            Email = t.User.Email,
            Subjects = t.TeacherSubjects.Select(ts => new SubjectDTO 
            { 
                Id = ts.Subject.Id, 
                Name = ts.Subject.Name 
            }).ToList(),
            PreparationPrograms = t.PreparationPrograms.Select(pp => new PreparationProgramDTO 
            { 
                Id = pp.Id, 
                Name = pp.Name,
                Description = pp.Description ?? string.Empty
            }).ToList(),
            User = new UserDTO
            {
                Id = t.User.Id,
                FullName = $"{t.User.LastName} {t.User.FirstName} {t.User.MiddleName}".Trim(),
                Email = t.User.Email,
                PhotoBase64 = t.User.PhotoBase64
            }
        });
    }

    public async Task<(bool Success, string Message)> SendRequestToTeacherAsync(int studentId, int teacherId)
    {
        _studentLogger.LogInformation("Отправка запроса: студент {StudentId} -> репетитор {TeacherId}", studentId, teacherId);
        
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
            return (false, "Студент не найден");
            
        var teacher = await _context.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == teacherId);
            
        if (teacher == null)
        {
            _studentLogger.LogWarning("Репетитор с UserId={TeacherId} не найден", teacherId);
            return (false, "Репетитор не найден");
        }
        
        _studentLogger.LogInformation("Найден профиль репетитора с ID: {ProfileId} для UserId: {UserId}", 
            teacher.Id, teacher.UserId);
            
        // Проверяем, не отправлял ли студент уже заявку этому репетитору
        var existingRequest = await _context.TeacherStudents
            .FirstOrDefaultAsync(ts => ts.StudentId == studentId && ts.TeacherId == teacher.Id && ts.Status == RequestStatus.Pending);
            
        if (existingRequest != null)
            return (false, "Заявка уже отправлена этому репетитору");
            
        // Проверяем, не учится ли уже студент у этого репетитора
        var existingRelation = await _context.TeacherStudents
            .FirstOrDefaultAsync(ts => ts.StudentId == studentId && ts.TeacherId == teacher.Id && ts.Status == RequestStatus.Accepted);
            
        if (existingRelation != null)
            return (false, "Вы уже являетесь учеником этого репетитора");
            
        // Создаем новую заявку как TeacherStudent с Pending статусом
        var request = new TeacherStudent
        {
            StudentId = studentId,
            TeacherId = teacher.Id, // Используем Id профиля, а не UserId
            RequestDate = DateTime.UtcNow,
            Status = RequestStatus.Pending
        };
        
        await _context.TeacherStudents.AddAsync(request);
        await _context.SaveChangesAsync();
        
        _studentLogger.LogInformation("Заявка успешно создана: StudentId={StudentId}, TeacherId={TeacherId}", 
            studentId, teacher.Id);
            
        return (true, "Заявка успешно отправлена репетитору");
    }

    public async Task<(bool Success, string Message)> UploadLessonAttachmentAsync(
        int studentId,
        int lessonId,
        string fileName,
        string fileType,
        string base64Content)
    {
        // Проверяем, существует ли студент
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
            return (false, "Студент не найден");
            
        // Проверяем, существует ли урок
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.StudentId == studentId);
            
        if (lesson == null)
            return (false, "Урок не найден или не принадлежит данному студенту");
            
        // Рассчитываем размер файла в килобайтах
        var base64WithoutPrefix = base64Content;
        if (base64Content.Contains(","))
        {
            base64WithoutPrefix = base64Content.Split(',')[1];
        }
        
        var fileBytes = Convert.FromBase64String(base64WithoutPrefix);
        var fileSizeKb = fileBytes.Length / 1024;
        
        // Создаем новое вложение
        var attachment = new Attachment
        {
            LessonId = lessonId,
            FileName = fileName,
            FileType = fileType,
            Base64Content = base64Content,
            Size = fileSizeKb
        };
        
        await _context.Attachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
        
        return (true, "Файл успешно загружен");
    }

    // Метод для получения файлов урока
    public async Task<IEnumerable<AttachmentDTO>> GetLessonAttachmentsAsync(int lessonId)
    {
        var attachments = await _context.Attachments
            .Where(a => a.LessonId == lessonId)
            .ToListAsync();
            
        return attachments.Select(a => new AttachmentDTO
        {
            Id = a.Id,
            FileName = a.FileName,
            FileType = a.FileType,
            Size = a.Size,
            Base64Content = a.Base64Content
        });
    }

    /// <summary>
    /// Получает урок по ID с автоматическим обновлением статуса, если он завершился
    /// </summary>
    /// <param name="studentId">ID пользователя-студента</param>
    /// <param name="lessonId">ID урока</param>
    /// <returns>Данные урока или null, если урок не найден</returns>
    public async Task<LessonDTO?> GetLessonAsync(int studentId, int lessonId)
    {
        _studentLogger.LogInformation("Получение урока с ID: {LessonId} для студента с ID: {StudentId}", lessonId, studentId);
        
        // Проверяем, существует ли студент
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
        {
            _studentLogger.LogWarning("Студент с ID={StudentId} не найден", studentId);
            return null;
        }
        
        // Обновляем статус урока и получаем его данные
        var lesson = await UpdateLessonStatusAsync(lessonId);
        
        if (lesson == null || lesson.StudentId != studentId)
        {
            _studentLogger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит студенту с ID={StudentId}", lessonId, studentId);
            return null;
        }
        
        return new LessonDTO
        {
            Id = lesson.Id,
            TeacherId = lesson.TeacherId,
            StudentId = lesson.StudentId ?? 0,
            SubjectId = lesson.SubjectId,
            TeacherName = $"{lesson.Teacher.LastName} {lesson.Teacher.FirstName}".Trim(),
            StudentName = $"{lesson.Student.LastName} {lesson.Student.FirstName}".Trim(),
            SubjectName = lesson.Subject.Name,
            StartTime = lesson.StartTime, // Используем время как есть, без изменения Kind
            EndTime = lesson.EndTime,     // Используем время как есть, без изменения Kind
            Status = lesson.ActualStatusString,
            ConferenceLink = lesson.ConferenceLink ?? string.Empty,
            BoardLink = lesson.WhiteboardLink ?? string.Empty
        };
    }
    
    /// <summary>
    /// Отменяет урок (меняет его статус на Cancelled)
    /// </summary>
    /// <param name="studentId">ID пользователя-студента</param>
    /// <param name="lessonId">ID урока для отмены</param>
    /// <returns>Результат операции и сообщение</returns>
    public async Task<(bool Success, string Message)> CancelLessonAsync(int studentId, int lessonId)
    {
        _studentLogger.LogInformation("Запрос на отмену урока с ID: {LessonId} от студента с ID: {StudentId}", lessonId, studentId);
        
        // Проверяем, существует ли студент
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
        {
            _studentLogger.LogWarning("Студент с ID={StudentId} не найден", studentId);
            return (false, "Студент не найден");
        }
        
        // Обновляем статус урока и получаем его данные
        var lesson = await UpdateLessonStatusAsync(lessonId);
        
        if (lesson == null || lesson.StudentId != studentId)
        {
            _studentLogger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит студенту с ID={StudentId}", lessonId, studentId);
            return (false, "Урок не найден или не принадлежит данному студенту");
        }
        
        // Проверяем, можно ли отменить урок (только для запланированных уроков)
        if (lesson.Status == LessonStatus.Completed)
        {
            _studentLogger.LogWarning("Невозможно отменить завершенный урок с ID={LessonId}", lessonId);
            return (false, "Невозможно отменить завершенный урок");
        }
        
        if (lesson.Status == LessonStatus.Cancelled)
        {
            _studentLogger.LogWarning("Урок с ID={LessonId} уже отменен", lessonId);
            return (false, "Урок уже отменен");
        }
        
        // Отменяем урок
        lesson.Status = LessonStatus.Cancelled;
        await _context.SaveChangesAsync();
        
        _studentLogger.LogInformation("Урок с ID={LessonId} успешно отменен", lessonId);
        return (true, "Урок успешно отменен");
    }
} 