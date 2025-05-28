using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using Vibetech.Educat.Services.DTO;
using System.IO;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Vibetech.Educat.Services.Services;

public class TeacherService : BaseService
{
    private readonly ILogger<TeacherService> _teacherLogger;

    public TeacherService(EducatDbContext context, ILogger<TeacherService> logger)
        : base(context, logger)
    {
        _teacherLogger = logger;
    }

    public async Task<TeacherProfileDTO> GetTeacherProfileAsync(int teacherId)
    {
        _teacherLogger.LogInformation("Поиск профиля преподавателя с ID: {TeacherId}", teacherId);
        
        // Сначала проверим, существует ли пользователь с таким ID
        var user = await _context.Users.FindAsync(teacherId);
        if (user == null)
        {
            _teacherLogger.LogWarning("Пользователь с ID: {TeacherId} не найден в базе данных", teacherId);
            return null!;
        }
        
        _teacherLogger.LogInformation("Пользователь найден. Email: {Email}, Роль: {Role}", user.Email, user.Role);
        
        var teacherProfile = await _context.TeacherProfiles
            .Include(tp => tp.User)
            .Include(tp => tp.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .Include(tp => tp.PreparationPrograms)
            .Include(tp => tp.Reviews)
            .Include(tp => tp.Students)
                .ThenInclude(s => s.Student)
            .Include(tp => tp.TeacherLessons)
            .FirstOrDefaultAsync(t => t.UserId == teacherId);
        
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Профиль преподавателя не найден для пользователя с ID: {TeacherId}", teacherId);
            return null!;
        }
        
        _teacherLogger.LogInformation("Профиль преподавателя найден. ID: {TeacherProfileId}", teacherProfile.Id);
        
        var result = new TeacherProfileDTO
        {
            UserId = teacherProfile.UserId,
            FullName = $"{teacherProfile.User.LastName} {teacherProfile.User.FirstName} {teacherProfile.User.MiddleName}".Trim(),
            Education = teacherProfile.Education,
            ExperienceYears = teacherProfile.ExperienceYears,
            HourlyRate = teacherProfile.HourlyRate,
            Rating = teacherProfile.Rating,
            ReviewsCount = teacherProfile.ReviewsCount,
            PhotoBase64 = teacherProfile.User.PhotoBase64,
            Email = teacherProfile.User.Email,
            Subjects = teacherProfile.TeacherSubjects.Select(ts => new SubjectDTO
            {
                Id = ts.Subject.Id,
                Name = ts.Subject.Name
            }).ToList(),
            PreparationPrograms = teacherProfile.PreparationPrograms.Select(pp => new PreparationProgramDTO
            {
                Id = pp.Id,
                Name = pp.Name,
                Description = pp.Description ?? string.Empty
            }).ToList(),
            User = new UserDTO
            {
                Id = teacherProfile.User.Id,
                FullName = $"{teacherProfile.User.LastName} {teacherProfile.User.FirstName} {teacherProfile.User.MiddleName}".Trim(),
                Email = teacherProfile.User.Email,
                PhotoBase64 = teacherProfile.User.PhotoBase64,
                BirthDate = teacherProfile.User.BirthDate,
                Gender = teacherProfile.User.Gender,
                ContactInfo = teacherProfile.User.ContactInformation,
                LastName = teacherProfile.User.LastName,
                FirstName = teacherProfile.User.FirstName,
                MiddleName = teacherProfile.User.MiddleName ?? string.Empty
            }
        };
        
        return result;
    }

    public async Task<TeacherProfileDTO> UpdateTeacherProfileAsync(
        int teacherId,
        string firstName,
        string lastName,
        string middleName,
        DateTime birthDate,
        string gender,
        string contactInfo,
        string education,
        int experienceYears,
        decimal hourlyRate,
        List<int> subjectIds,
        List<int> preparationProgramIds,
        string photoBase64 = null)
    {
        _teacherLogger.LogInformation("Обновление профиля репетитора с ID: {TeacherId}", teacherId);
        
        // Обновляем информацию о пользователе
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role == "Teacher");
            
        if (user == null)
        {
            _teacherLogger.LogWarning("Пользователь-репетитор с ID: {TeacherId} не найден", teacherId);
            return null!;
        }
            
        user.FirstName = firstName;
        user.LastName = lastName;
        user.MiddleName = middleName;
        
        // Явно устанавливаем UTC для даты рождения
        user.BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);
        
        user.Gender = gender;
        user.ContactInformation = contactInfo;
        
        if (!string.IsNullOrEmpty(photoBase64))
        {
            // Обновляем фото пользователя
            user.PhotoBase64 = photoBase64;
        }
        
        // Обновляем профиль репетитора
        var teacherProfile = await _context.TeacherProfiles
            .Include(tp => tp.TeacherSubjects)
            .Include(tp => tp.PreparationPrograms)
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Профиль репетитора для пользователя с ID: {TeacherId} не найден", teacherId);
            return null!;
        }
            
        teacherProfile.Education = education;
        teacherProfile.ExperienceYears = experienceYears;
        teacherProfile.HourlyRate = hourlyRate;
        
        // Обновляем предметы репетитора
        _teacherLogger.LogInformation("Обновление предметов для репетитора. Текущее количество: {Count}, новое количество: {NewCount}", 
            teacherProfile.TeacherSubjects.Count, subjectIds.Count);
            
        // Находим предметы, которые нужно удалить (есть в базе, но отсутствуют в запросе)
        var subjectsToRemove = teacherProfile.TeacherSubjects
            .Where(ts => !subjectIds.Contains(ts.SubjectId))
            .ToList();
            
        // Находим предметы, которые нужно добавить (есть в запросе, но отсутствуют в базе)
        var existingSubjectIds = teacherProfile.TeacherSubjects.Select(ts => ts.SubjectId).ToList();
        var subjectsToAdd = subjectIds.Where(id => !existingSubjectIds.Contains(id)).ToList();
        
        // Удаляем неактуальные предметы
        if (subjectsToRemove.Any())
        {
            _teacherLogger.LogInformation("Удаление {Count} предметов", subjectsToRemove.Count);
            _context.TeacherSubjects.RemoveRange(subjectsToRemove);
        }
        
        // Добавляем новые предметы
        if (subjectsToAdd.Any())
        {
            _teacherLogger.LogInformation("Добавление {Count} новых предметов", subjectsToAdd.Count);
            foreach (var subjectId in subjectsToAdd)
            {
                teacherProfile.TeacherSubjects.Add(new TeacherSubject
                {
                    TeacherProfileId = teacherProfile.Id,
                    SubjectId = subjectId
                });
            }
        }
        
        // Обновляем программы подготовки
        _teacherLogger.LogInformation("Обновление программ подготовки. Текущее количество: {Count}, новое количество: {NewCount}",
            teacherProfile.PreparationPrograms.Count, preparationProgramIds.Count);
            
        // Получаем список ID существующих программ
        var existingProgramIds = await _context.PreparationPrograms
            .Where(p => preparationProgramIds.Contains(p.Id) && p.TeacherProfileId == null)
            .Select(p => p.Id)
            .ToListAsync();
            
        // Находим программы, которые нужно удалить
        var programsToRemove = teacherProfile.PreparationPrograms
            .Where(p => !preparationProgramIds.Contains(p.Id))
            .ToList();
            
        // Удаляем неактуальные программы
        if (programsToRemove.Any())
        {
            _teacherLogger.LogInformation("Удаление {Count} программ подготовки", programsToRemove.Count);
            _context.PreparationPrograms.RemoveRange(programsToRemove);
        }
        
        // Находим программы, которые нужно добавить
        var existingTeacherProgramIds = teacherProfile.PreparationPrograms.Select(p => p.Id).ToList();
        var programsToAdd = preparationProgramIds.Where(id => !existingTeacherProgramIds.Contains(id)).ToList();
        
        // Добавляем программы к профилю учителя
        if (programsToAdd.Any())
        {
            _teacherLogger.LogInformation("Добавление {Count} новых программ подготовки", programsToAdd.Count);
            
            // Получаем существующие программы из базы данных
            var programsFromDb = await _context.PreparationPrograms
                .Where(p => programsToAdd.Contains(p.Id))
                .ToListAsync();
                
            foreach (var program in programsFromDb)
            {
                // Связываем программу с профилем учителя
                program.TeacherProfileId = teacherProfile.Id;
                program.TeacherProfile = teacherProfile;
            }
            
            // Для ID, которых нет в базе, создаем новые записи
            var missingProgramIds = programsToAdd.Except(programsFromDb.Select(p => p.Id)).ToList();
            foreach (var programId in missingProgramIds)
            {
                _teacherLogger.LogWarning("Программа подготовки с ID {ProgramId} не найдена в базе данных, создаем новую", programId);
                teacherProfile.PreparationPrograms.Add(new PreparationProgram
                {
                    Id = programId, // Пытаемся сохранить запрошенный ID
                    TeacherProfileId = teacherProfile.Id,
                    Name = $"Программа {programId}",
                    Description = "Автоматически созданная программа"
                });
            }
        }
        
        await _context.SaveChangesAsync();
        _teacherLogger.LogInformation("Профиль репетитора с ID: {TeacherId} успешно обновлен", teacherId);
        
        // Загружаем обновленные данные для возврата
        return await GetTeacherProfileAsync(teacherId);
    }

    public async Task<IEnumerable<int>> GetTeacherStudentsAsync(int teacherId)
    {
        _teacherLogger.LogInformation("Получение студентов для учителя с ID: {TeacherId}", teacherId);
        
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Профиль учителя с UserId={TeacherId} не найден", teacherId);
            return Enumerable.Empty<int>();
        }
        
        _teacherLogger.LogInformation("Найден профиль учителя с ID: {ProfileId} для UserId: {UserId}", 
            teacherProfile.Id, teacherProfile.UserId);
            
        // Получаем студентов через их User ID (не StudenId из TeacherStudents)
        var studentUserIds = await _context.TeacherStudents
            .Join(_context.Users,
                ts => ts.StudentId,
                user => user.Id,
                (ts, user) => new { UserId = user.Id, TeacherId = ts.TeacherId, Status = ts.Status })
            .Where(x => x.TeacherId == teacherProfile.Id && x.Status == RequestStatus.Accepted)
            .Select(x => x.UserId)
            .ToListAsync();
        
        _teacherLogger.LogInformation("Найдено {Count} студентов для учителя", studentUserIds.Count);
        foreach (var userId in studentUserIds)
        {
            _teacherLogger.LogInformation("UserId студента: {UserId}", userId);
        }
        
        return studentUserIds;
    }

    public async Task<IEnumerable<LessonDTO>> GetTeacherLessonsAsync(
        int teacherId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        _teacherLogger.LogInformation("Получение уроков репетитора с ID: {TeacherId}", teacherId);
        
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
            return Enumerable.Empty<LessonDTO>();
        }
        
        // Обновляем статусы всех уроков
        await UpdateAllLessonsStatusAsync();
        
        // Получаем уроки репетитора
        var query = _context.Lessons
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.TeacherId == teacherId);
        
        // Фильтрация по дате начала
        if (startDate.HasValue)
        {
            _teacherLogger.LogInformation("Фильтрация по начальной дате: {StartDate}", startDate.Value);
            
            // Предполагаем, что даты приходят в локальном времени, конвертируем их в UTC для правильного сравнения
            DateTime startDateUtc = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.StartTime >= startDateUtc);
        }
        
        // Фильтрация по дате окончания
        if (endDate.HasValue)
        {
            _teacherLogger.LogInformation("Фильтрация по конечной дате: {EndDate}", endDate.Value);
            
            // Предполагаем, что даты приходят в локальном времени, конвертируем их в UTC для правильного сравнения
            DateTime endDateUtc = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.StartTime <= endDateUtc);
        }
        
        // Получаем уроки
        var lessons = await query
            .OrderBy(l => l.StartTime)
            .ToListAsync();
            
        _teacherLogger.LogInformation("Найдено {Count} уроков для репетитора {TeacherId}", lessons.Count, teacherId);
        
        // Формируем DTO
        return lessons.Select(l => new LessonDTO
        {
            Id = l.Id,
            TeacherId = l.TeacherId,
            StudentId = l.StudentId ?? 0,
            SubjectId = l.SubjectId,
            TeacherName = $"{l.Teacher.LastName} {l.Teacher.FirstName}".Trim(),
            StudentName = $"{l.Student.LastName} {l.Student.FirstName}".Trim(),
            SubjectName = l.Subject.Name,
            StartTime = l.StartTime,
            EndTime = l.EndTime,
            Status = l.ActualStatusString,
            ConferenceLink = l.ConferenceLink ?? string.Empty,
            BoardLink = l.WhiteboardLink ?? string.Empty
        });
    }

    public async Task<TeacherStatisticsDTO> GetTeacherStatisticsAsync(int teacherId)
    {
        // Получаем профиль репетитора
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
            return null!;
            
        // Количество учеников
        var studentsCount = await _context.TeacherStudents
            .Where(ts => ts.TeacherId == teacherProfile.Id)
            .CountAsync();
            
        // Получаем уроки репетитора
        var lessons = await _context.Lessons
            .Where(l => l.TeacherId == teacherProfile.Id)
            .ToListAsync();
            
        var totalLessons = lessons.Count;
        var completedLessons = lessons.Count(l => l.Status == LessonStatus.Completed);
        var upcomingLessons = lessons.Count(l => l.Status == LessonStatus.Scheduled && l.StartTime > DateTime.UtcNow);
        
        // Распределение уроков по предметам
        var lessonsBySubject = lessons
            .GroupBy(l => l.SubjectId)
            .ToDictionary(g => g.Key, g => g.Count());
            
        // Распределение оценок
        var ratingDistribution = await _context.Reviews
            .Where(r => r.TeacherId == teacherProfile.UserId)
            .GroupBy(r => (int)r.Rating)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
            
        // Если нет отзывов с какой-то оценкой, добавляем нули
        for (int i = 1; i <= 5; i++)
        {
            if (!ratingDistribution.ContainsKey(i))
                ratingDistribution[i] = 0;
        }
        
        return new TeacherStatisticsDTO
        {
            TotalStudents = studentsCount,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            UpcomingLessons = upcomingLessons,
            Rating = teacherProfile.Rating,
            ReviewsCount = teacherProfile.ReviewsCount,
            LessonsBySubject = lessonsBySubject,
            RatingDistribution = ratingDistribution
        };
    }

    public async Task<IEnumerable<StudentRequestDTO>> GetTeacherRequestsAsync(int teacherId)
    {
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
            return Enumerable.Empty<StudentRequestDTO>();
            
        var requests = await _context.TeacherStudents
            .Include(ts => ts.Student)
            .Include(ts => ts.Teacher)
            .Where(ts => ts.TeacherId == teacherProfile.Id && ts.Status == RequestStatus.Pending)
            .ToListAsync();
            
        return requests.Select(r => new StudentRequestDTO
        {
            Id = r.Id,
            StudentId = r.StudentId,
            TeacherId = r.TeacherId,
            StudentName = $"{r.Student.LastName} {r.Student.FirstName} {r.Student.MiddleName}".Trim(),
            TeacherName = $"{r.Teacher.LastName} {r.Teacher.FirstName} {r.Teacher.MiddleName}".Trim(),
            RequestDate = r.RequestDate,
            Status = r.Status.ToString()
        });
    }

    public async Task<(bool Success, string Message)> AcceptStudentRequestAsync(int requestId)
    {
        var request = await _context.TeacherStudents
            .Include(ts => ts.Student)
            .Include(ts => ts.Teacher)
            .FirstOrDefaultAsync(ts => ts.Id == requestId);
            
        if (request == null)
            return (false, "Заявка не найдена");
            
        if (request.Status != RequestStatus.Pending)
            return (false, "Заявка уже обработана");
            
        // Обновляем статус заявки и дату принятия
        request.Status = RequestStatus.Accepted;
        request.AcceptedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return (true, "Заявка успешно принята");
    }

    public async Task<(bool Success, string Message)> RejectStudentRequestAsync(int requestId)
    {
        var request = await _context.TeacherStudents
            .FirstOrDefaultAsync(ts => ts.Id == requestId);
            
        if (request == null)
            return (false, "Заявка не найдена");
            
        if (request.Status != RequestStatus.Pending)
            return (false, "Заявка уже обработана");
            
        // Обновляем статус заявки
        request.Status = RequestStatus.Rejected;
        
        await _context.SaveChangesAsync();
        
        return (true, "Заявка успешно отклонена");
    }

    public async Task<(bool Success, string Message)> RemoveStudentAsync(int teacherId, int studentId)
    {
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
            return (false, "Репетитор не найден");
            
        // Находим все отношения между учителем и студентом, независимо от статуса
        var relations = await _context.TeacherStudents
            .Where(ts => ts.TeacherId == teacherProfile.Id && ts.StudentId == studentId)
            .ToListAsync();
            
        if (!relations.Any())
            return (false, "Ученик не найден у данного репетитора");
            
        // Удаляем все найденные отношения
        _context.TeacherStudents.RemoveRange(relations);
        await _context.SaveChangesAsync();
        
        return (true, "Ученик успешно удален");
    }

    public async Task<(bool Success, string Message, LessonDTO Lesson)> CreateLessonAsync(
        int teacherId,
        int studentId,
        int subjectId,
        DateTime startTime,
        DateTime endTime,
        string conferenceLink,
        string boardLink)
    {
        _teacherLogger.LogInformation("Создание урока: учитель {TeacherId}, студент {StudentId}, предмет {SubjectId}, время {StartTime} - {EndTime}",
            teacherId, studentId, subjectId, startTime, endTime);
            
        // Проверяем, существует ли репетитор
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Репетитор с UserId={TeacherId} не найден", teacherId);
            return (false, "Репетитор не найден", null!);
        }
            
        // Проверяем, существует ли ученик
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
        if (student == null)
        {
            _teacherLogger.LogWarning("Ученик с ID={StudentId} не найден", studentId);
            return (false, "Ученик не найден", null!);
        }
            
        // Проверяем, есть ли отношение учитель-ученик
        var relation = await _context.TeacherStudents
            .FirstOrDefaultAsync(ts => ts.TeacherId == teacherProfile.Id && ts.StudentId == studentId);
            
        if (relation == null)
        {
            _teacherLogger.LogWarning("Ученик {StudentId} не прикреплен к репетитору {TeacherId}", studentId, teacherId);
            return (false, "Данный ученик не прикреплен к репетитору", null!);
        }
            
        // Проверяем, существует ли предмет
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId);
            
        if (subject == null)
        {
            _teacherLogger.LogWarning("Предмет с ID={SubjectId} не найден", subjectId);
            return (false, "Предмет не найден", null!);
        }
        
        // Примечание: проверки на конфликт по времени удалены для возможности создания пересекающихся уроков
        
        // Предполагаем, что даты приходят в локальном времени, конвертируем их в UTC для хранения
        DateTime startTimeUtc = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        DateTime endTimeUtc = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
        
        // Создаем новый урок с сохранением времени в UTC
        var lesson = new Lesson
        {
            TeacherId = teacherId,
            StudentId = studentId,
            SubjectId = subjectId,
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            Status = LessonStatus.Scheduled,
            ConferenceLink = conferenceLink,
            WhiteboardLink = boardLink
        };
        
        await _context.Lessons.AddAsync(lesson);
        await _context.SaveChangesAsync();
        
        _teacherLogger.LogInformation("Урок успешно создан с ID: {LessonId}", lesson.Id);
        
        // Загружаем данные для ответа
        var createdLesson = await _context.Lessons
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .FirstOrDefaultAsync(l => l.Id == lesson.Id);
            
        if (createdLesson == null)
        {
            _teacherLogger.LogError("Не удалось получить созданный урок с ID: {LessonId}", lesson.Id);
            return (false, "Не удалось получить созданный урок", null!);
        }
        
        return (true, "Урок успешно создан", new LessonDTO
        {
            Id = createdLesson.Id,
            TeacherId = createdLesson.TeacherId,
            StudentId = createdLesson.StudentId ?? 0,
            SubjectId = createdLesson.SubjectId,
            TeacherName = $"{createdLesson.Teacher.LastName} {createdLesson.Teacher.FirstName}".Trim(),
            StudentName = $"{createdLesson.Student.LastName} {createdLesson.Student.FirstName}".Trim(),
            SubjectName = createdLesson.Subject.Name,
            StartTime = createdLesson.StartTime,
            EndTime = createdLesson.EndTime,
            Status = createdLesson.ActualStatusString,
            ConferenceLink = createdLesson.ConferenceLink ?? string.Empty,
            BoardLink = createdLesson.WhiteboardLink ?? string.Empty
        });
    }

    public async Task<(bool Success, string Message)> UploadLessonAttachmentAsync(
        int teacherId,
        int lessonId,
        string fileName,
        string fileType,
        string base64Content)
    {
        // Проверяем, существует ли репетитор
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
            return (false, "Репетитор не найден");
            
        // Проверяем, существует ли урок
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.TeacherId == teacherId);
            
        if (lesson == null)
            return (false, "Урок не найден или не принадлежит данному репетитору");
            
        try
        {
            // Рассчитываем размер файла в килобайтах
            var base64WithoutPrefix = base64Content;
            if (base64Content.Contains(","))
            {
                base64WithoutPrefix = base64Content.Split(',')[1];
            }
            
            var fileBytes = Convert.FromBase64String(base64WithoutPrefix);
            var fileSizeKb = fileBytes.Length / 1024;
            
            // Создаем новое вложение с информацией о файле
            var attachment = new Attachment
            {
                LessonId = lessonId,
                FileName = fileName,
                FileType = fileType,
                Size = fileSizeKb,
                Base64Content = base64Content
            };
            
            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
            
            return (true, "Файл успешно загружен");
        }
        catch (Exception ex)
        {
            _teacherLogger.LogError(ex, "Ошибка при загрузке файла: {Message}", ex.Message);
            return (false, $"Ошибка при загрузке файла: {ex.Message}");
        }
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
    /// <param name="teacherId">ID пользователя-репетитора</param>
    /// <param name="lessonId">ID урока</param>
    /// <returns>Данные урока или null, если урок не найден</returns>
    public async Task<LessonDTO?> GetLessonAsync(int teacherId, int lessonId)
    {
        _teacherLogger.LogInformation("Получение урока с ID: {LessonId} для репетитора с ID: {TeacherId}", lessonId, teacherId);
        
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
            return null;
        }
        
        // Обновляем статус урока и получаем его данные
        var lesson = await UpdateLessonStatusAsync(lessonId);
        
        if (lesson == null || lesson.TeacherId != teacherId)
        {
            _teacherLogger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит репетитору с ID={TeacherId}", lessonId, teacherId);
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
            StartTime = lesson.StartTime,
            EndTime = lesson.EndTime,
            Status = lesson.ActualStatusString,
            ConferenceLink = lesson.ConferenceLink ?? string.Empty,
            BoardLink = lesson.WhiteboardLink ?? string.Empty
        };
    }

    /// <summary>
    /// Отменяет урок (меняет его статус на Cancelled)
    /// </summary>
    /// <param name="teacherId">ID пользователя-репетитора</param>
    /// <param name="lessonId">ID урока для отмены</param>
    /// <returns>Результат операции и сообщение</returns>
    public async Task<(bool Success, string Message)> CancelLessonAsync(int teacherId, int lessonId)
    {
        _teacherLogger.LogInformation("Запрос на отмену урока с ID: {LessonId} от репетитора с ID: {TeacherId}", lessonId, teacherId);
        
        // Проверяем, существует ли репетитор
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _teacherLogger.LogWarning("Репетитор с UserId={TeacherId} не найден", teacherId);
            return (false, "Репетитор не найден");
        }
        
        // Обновляем статус урока и получаем его данные
        var lesson = await UpdateLessonStatusAsync(lessonId);
        
        if (lesson == null || lesson.TeacherId != teacherId)
        {
            _teacherLogger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит репетитору с ID={TeacherId}", lessonId, teacherId);
            return (false, "Урок не найден или не принадлежит данному репетитору");
        }
        
        // Проверяем, можно ли отменить урок (только для запланированных уроков)
        if (lesson.Status == LessonStatus.Completed)
        {
            _teacherLogger.LogWarning("Невозможно отменить завершенный урок с ID={LessonId}", lessonId);
            return (false, "Невозможно отменить завершенный урок");
        }
        
        if (lesson.Status == LessonStatus.Cancelled)
        {
            _teacherLogger.LogWarning("Урок с ID={LessonId} уже отменен", lessonId);
            return (false, "Урок уже отменен");
        }
        
        // Отменяем урок
        lesson.Status = LessonStatus.Cancelled;
        await _context.SaveChangesAsync();
        
        _teacherLogger.LogInformation("Урок с ID={LessonId} успешно отменен", lessonId);
        return (true, "Урок успешно отменен");
    }

    /// <summary>
    /// Обновляет статусы всех уроков для всех пользователей
    /// </summary>
    /// <returns>Количество обновленных уроков</returns>
    public async Task<int> UpdateAllLessonsStatusAsync()
    {
        _teacherLogger.LogInformation("Ручное обновление статусов всех уроков");
        
        var now = DateTime.UtcNow;
        
        // Находим все запланированные уроки, которые уже должны быть завершены
        var completedLessons = await _context.Lessons
            .Where(l => l.Status == LessonStatus.Scheduled && l.EndTime < now)
            .ToListAsync();
            
        // Находим все запланированные уроки, которые должны быть в процессе
        var inProgressLessons = await _context.Lessons
            .Where(l => l.Status == LessonStatus.Scheduled && l.StartTime <= now && l.EndTime > now)
            .ToListAsync();
            
        int totalUpdated = 0;
            
        if (completedLessons.Any())
        {
            // Обновляем статусы на Completed
            foreach (var lesson in completedLessons)
            {
                lesson.Status = LessonStatus.Completed;
            }
            
            totalUpdated += completedLessons.Count;
            _teacherLogger.LogInformation("Автоматически обновлены статусы {Count} уроков на Completed", completedLessons.Count);
        }
        
        if (inProgressLessons.Any())
        {
            // Обновляем статусы на InProgress
            foreach (var lesson in inProgressLessons)
            {
                lesson.Status = LessonStatus.InProgress;
            }
            
            totalUpdated += inProgressLessons.Count;
            _teacherLogger.LogInformation("Автоматически обновлены статусы {Count} уроков на InProgress", inProgressLessons.Count);
        }
        
        // Сохраняем изменения, если они есть
        if (totalUpdated > 0)
        {
            await _context.SaveChangesAsync();
        }
        
        return totalUpdated;
    }
} 