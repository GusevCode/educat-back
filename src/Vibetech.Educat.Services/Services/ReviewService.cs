using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using Vibetech.Educat.Services.DTO;

namespace Vibetech.Educat.Services.Services;

public class ReviewService : BaseService
{
    private readonly ILogger<ReviewService> _reviewLogger;

    public ReviewService(EducatDbContext context, ILogger<ReviewService> logger)
        : base(context, logger)
    {
        _reviewLogger = logger;
    }

    /// <summary>
    /// Получение отзывов репетитора
    /// </summary>
    public async Task<IEnumerable<ReviewDTO>> GetTeacherReviewsAsync(int teacherId)
    {
        _reviewLogger.LogInformation("Получение отзывов для репетитора с ID: {TeacherId}", teacherId);
        
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
            
        if (teacherProfile == null)
        {
            _reviewLogger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
            return Enumerable.Empty<ReviewDTO>();
        }
        
        var reviews = await _context.Reviews
            .Include(r => r.Teacher)
            .Include(r => r.Student)
            .Where(r => r.TeacherId == teacherId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
            
        _reviewLogger.LogInformation("Найдено {Count} отзывов для репетитора", reviews.Count);
        
        return reviews.Select(r => new ReviewDTO
        {
            Id = r.Id,
            LessonId = r.LessonId,
            TeacherId = r.TeacherId,
            StudentId = r.StudentId,
            TeacherName = $"{r.Teacher.LastName} {r.Teacher.FirstName}".Trim(),
            StudentName = $"{r.Student.LastName} {r.Student.FirstName}".Trim(),
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    /// <summary>
    /// Создание отзыва о репетиторе
    /// </summary>
    public async Task<(bool Success, string Message, ReviewDTO? Review)> CreateReviewAsync(
        int lessonId, int teacherId, int studentId, int rating, string comment)
    {
        try
        {
            _reviewLogger.LogInformation("Создание отзыва: урок {LessonId}, репетитор {TeacherId}, студент {StudentId}, оценка {Rating}", 
                lessonId, teacherId, studentId, rating);
            
            // Проверяем существование урока
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                _reviewLogger.LogWarning("Урок с ID={LessonId} не найден", lessonId);
                return (false, "Урок не найден", null);
            }
            
            // Обновляем статус урока и получаем его данные
            lesson = await UpdateLessonStatusAsync(lessonId);
                
            if (lesson == null)
            {
                _reviewLogger.LogWarning("Не удалось получить данные урока с ID={LessonId}", lessonId);
                return (false, "Не удалось получить данные урока", null);
            }
            
            _reviewLogger.LogInformation("Урок найден: ID={LessonId}, Статус={Status}, Студент={StudentId}, Учитель={TeacherId}", 
                lesson.Id, lesson.Status, lesson.StudentId, lesson.TeacherId);
            
            // Проверяем, завершен ли урок
            if (lesson.Status != LessonStatus.Completed)
            {
                // Если статус не Completed, но время урока уже прошло, 
                // автоматически обновляем статус на Completed
                if (lesson.Status == LessonStatus.Scheduled && lesson.EndTime < DateTime.UtcNow)
                {
                    _reviewLogger.LogInformation("Автоматическое обновление статуса урока с ID={LessonId} на Completed, так как время окончания {EndTime} уже прошло", 
                        lessonId, lesson.EndTime);
                        
                    lesson.Status = LessonStatus.Completed;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _reviewLogger.LogWarning("Урок с ID={LessonId} не завершен. Статус: {Status}", lessonId, lesson.Status);
                    return (false, $"Можно оставить отзыв только о завершенном уроке. Текущий статус: {lesson.Status}", null);
                }
            }
            
            // Проверяем, принадлежит ли урок указанному студенту
            if (lesson.StudentId != studentId)
            {
                _reviewLogger.LogWarning("Урок с ID={LessonId} не принадлежит студенту с ID={StudentId}. Фактический StudentId={ActualStudentId}", 
                    lessonId, studentId, lesson.StudentId);
                return (false, "Урок не принадлежит указанному студенту", null);
            }
            
            // Получаем информацию об учителе
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null)
            {
                _reviewLogger.LogWarning("Учитель с ID={TeacherId} не найден", teacherId);
                return (false, "Учитель не найден", null);
            }
            
            // Получаем профиль репетитора
            var teacherProfile = await _context.TeacherProfiles
                .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
                
            if (teacherProfile == null)
            {
                _reviewLogger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
                return (false, "Профиль репетитора не найден", null);
            }
            
            // Проверяем, соответствует ли репетитор уроку
            _reviewLogger.LogInformation("Проверка соответствия учителя: Lesson.TeacherId={LessonTeacherId}, Request.TeacherId={RequestTeacherId}", 
                lesson.TeacherId, teacherId);
                
            if (lesson.TeacherId != teacherId)
            {
                _reviewLogger.LogWarning("Урок с ID={LessonId} не принадлежит репетитору с ID={TeacherId}",
                    lessonId, teacherId);
                return (false, "Урок не принадлежит указанному репетитору", null);
            }
            
            // Проверяем, не оставил ли студент уже отзыв об этом уроке
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.LessonId == lessonId && r.StudentId == studentId);
                
            if (existingReview != null)
            {
                _reviewLogger.LogWarning("Студент с ID={StudentId} уже оставил отзыв об уроке с ID={LessonId}, отзыв ID={ReviewId}", 
                    studentId, lessonId, existingReview.Id);
                return (false, "Вы уже оставили отзыв об этом уроке", null);
            }
            
            // Получаем студента
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
            {
                _reviewLogger.LogWarning("Студент с ID={StudentId} не найден", studentId);
                return (false, "Студент не найден", null);
            }
            
            // Создаем новый отзыв
            var review = new Review
            {
                LessonId = lessonId,
                TeacherId = teacherProfile.UserId,  // Используем ID пользователя учителя
                StudentId = studentId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
            
            _reviewLogger.LogInformation("Создание отзыва: LessonId={LessonId}, TeacherId={TeacherId}, StudentId={StudentId}, Rating={Rating}", 
                review.LessonId, review.TeacherId, review.StudentId, review.Rating);
            
            await _context.Reviews.AddAsync(review);
            
            // Обновляем рейтинг репетитора
            if (teacherProfile != null)
            {
                await UpdateTeacherRatingAsync(teacherProfile.Id);
                _reviewLogger.LogInformation("Обновлен рейтинг репетитора с ID профиля: {ProfileId}, UserId: {UserId}", 
                    teacherProfile.Id, teacherId);
            }
            
            await _context.SaveChangesAsync();
            
            _reviewLogger.LogInformation("Отзыв успешно создан с ID: {ReviewId}", review.Id);
            
            var reviewDto = new ReviewDTO
            {
                Id = review.Id,
                LessonId = review.LessonId,
                TeacherId = review.TeacherId,
                StudentId = review.StudentId,
                TeacherName = $"{teacher.LastName} {teacher.FirstName}".Trim(),
                StudentName = $"{student.LastName} {student.FirstName}".Trim(),
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
            
            return (true, "Отзыв успешно создан", reviewDto);
        }
        catch (Exception ex)
        {
            _reviewLogger.LogError(ex, "Произошла ошибка при создании отзыва: урок {LessonId}, репетитор {TeacherId}, студент {StudentId}", 
                lessonId, teacherId, studentId);
            return (false, $"Произошла ошибка при создании отзыва: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Получение отзывов, оставленных студентом
    /// </summary>
    public async Task<IEnumerable<ReviewDTO>> GetStudentReviewsAsync(int studentId)
    {
        _reviewLogger.LogInformation("Получение отзывов, оставленных студентом с ID: {StudentId}", studentId);
        
        var reviews = await _context.Reviews
            .Include(r => r.Teacher)
            .Include(r => r.Student)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
            
        _reviewLogger.LogInformation("Найдено {Count} отзывов от студента", reviews.Count);
        
        return reviews.Select(r => new ReviewDTO
        {
            Id = r.Id,
            LessonId = r.LessonId,
            TeacherId = r.TeacherId,
            StudentId = r.StudentId,
            TeacherName = $"{r.Teacher.LastName} {r.Teacher.FirstName}".Trim(),
            StudentName = $"{r.Student.LastName} {r.Student.FirstName}".Trim(),
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    /// <summary>
    /// Получение отзывов об уроке по его идентификатору
    /// </summary>
    public async Task<IEnumerable<ReviewDTO>> GetLessonReviewsAsync(int lessonId)
    {
        _reviewLogger.LogInformation("Получение отзывов для урока с ID: {LessonId}", lessonId);
        
        var reviews = await _context.Reviews
            .Include(r => r.Teacher)
            .Include(r => r.Student)
            .Where(r => r.LessonId == lessonId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
            
        _reviewLogger.LogInformation("Найдено {Count} отзывов для урока", reviews.Count);
        
        return reviews.Select(r => new ReviewDTO
        {
            Id = r.Id,
            LessonId = r.LessonId,
            TeacherId = r.TeacherId,
            StudentId = r.StudentId,
            TeacherName = $"{r.Teacher.LastName} {r.Teacher.FirstName}".Trim(),
            StudentName = $"{r.Student.LastName} {r.Student.FirstName}".Trim(),
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    /// <summary>
    /// Обновление рейтинга репетитора
    /// </summary>
    private async Task UpdateTeacherRatingAsync(int teacherProfileId)
    {
        _reviewLogger.LogInformation("Обновление рейтинга для профиля репетитора с ID: {TeacherProfileId}", teacherProfileId);
        
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.Id == teacherProfileId);
            
        if (teacherProfile == null)
        {
            _reviewLogger.LogWarning("Профиль репетитора с ID={TeacherProfileId} не найден", teacherProfileId);
            return;
        }
        
        // Получаем все отзывы о репетиторе (ищем по ID профиля репетитора)
        // В репозитории отзывы хранятся с TeacherId = UserId, поэтому используем UserId для поиска
        var reviews = await _context.Reviews
            .Where(r => r.TeacherId == teacherProfile.UserId)
            .ToListAsync();
            
        if (reviews.Count == 0)
        {
            _reviewLogger.LogInformation("Отзывы для репетитора с UserId={TeacherId} не найдены", teacherProfile.UserId);
            teacherProfile.Rating = 0;
            teacherProfile.ReviewsCount = 0;
        }
        else
        {
            // Рассчитываем средний рейтинг
            var averageRating = reviews.Average(r => r.Rating);
            
            teacherProfile.Rating = averageRating;
            teacherProfile.ReviewsCount = reviews.Count;
            
            _reviewLogger.LogInformation("Рейтинг репетитора обновлен: {Rating} (на основе {Count} отзывов)", 
                averageRating, reviews.Count);
        }
        
        await _context.SaveChangesAsync();
    }
} 