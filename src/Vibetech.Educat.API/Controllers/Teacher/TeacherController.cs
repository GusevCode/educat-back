using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vibetech.Educat.Services.Services;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.API.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Vibetech.Educat.API.Controllers.Teacher;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("API для репетиторов")]
public class TeacherController : ControllerBase
{
    private readonly TeacherService _teacherService;
    private readonly ILogger<TeacherController> _logger;
    private readonly IMapper _mapper;
    private readonly EducatDbContext _context;

    public TeacherController(TeacherService teacherService, ILogger<TeacherController> logger, IMapper mapper, EducatDbContext context)
    {
        _teacherService = teacherService;
        _logger = logger;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet("profile/{teacherId}")]
    [SwaggerOperation(Summary = "Получение профиля репетитора", Description = "Возвращает данные профиля репетитора по ID")]
    [SwaggerResponse(200, "Профиль репетитора", typeof(TeacherProfileDto))]
    [SwaggerResponse(404, "Репетитор не найден")]
    public async Task<IActionResult> GetTeacherProfile(int teacherId)
    {
        _logger.LogInformation("Получен запрос профиля репетитора с ID: {TeacherId}", teacherId);
        
        var profile = await _teacherService.GetTeacherProfileAsync(teacherId);
        
        if (profile == null)
        {
            _logger.LogWarning("Профиль репетитора с ID: {TeacherId} не найден", teacherId);
            return NotFound();
        }
        
        var mappedProfile = _mapper.Map<TeacherProfileDto>(profile);
        
        // Добавляем роли в UserDto, если он существует
        if (mappedProfile.User != null)
        {
            mappedProfile.User.IsTeacher = true;
            mappedProfile.User.Roles = new List<string> { "TEACHER" };
        }
        
        return Ok(mappedProfile);
    }

    [HttpPut("profile/{teacherId}")]
    [SwaggerOperation(Summary = "Обновление профиля репетитора", Description = "Обновляет данные профиля репетитора")]
    [SwaggerResponse(200, "Профиль репетитора успешно обновлен", typeof(TeacherProfileDto))]
    [SwaggerResponse(404, "Репетитор не найден")]
    public async Task<IActionResult> UpdateTeacherProfile(int teacherId, [FromBody] UpdateTeacherProfileRequest request)
    {
        var profile = await _teacherService.UpdateTeacherProfileAsync(teacherId, 
            request.FirstName, request.LastName, request.MiddleName,
            request.BirthDate, request.Gender, request.ContactInfo, 
            request.Education, request.ExperienceYears, request.HourlyRate,
            request.SubjectIds, request.PreparationProgramIds, request.PhotoBase64);
        
        if (profile == null)
            return NotFound();
            
        var mappedProfile = _mapper.Map<TeacherProfileDto>(profile);
        return Ok(mappedProfile);
    }

    [HttpGet("{teacherId}/students")]
    [SwaggerOperation(Summary = "Получение учеников репетитора", Description = "Возвращает список идентификаторов пользователей (UserId) учеников для указанного репетитора")]
    [SwaggerResponse(200, "Список идентификаторов пользователей учеников", typeof(IEnumerable<int>))]
    public async Task<IActionResult> GetTeacherStudents(int teacherId)
    {
        var studentIds = await _teacherService.GetTeacherStudentsAsync(teacherId);
        return Ok(studentIds);
    }

    [HttpGet("{teacherId}/lessons")]
    [SwaggerOperation(Summary = "Получение уроков репетитора", Description = "Возвращает список уроков репетитора с возможностью фильтрации по датам")]
    [SwaggerResponse(200, "Список уроков репетитора", typeof(IEnumerable<LessonDto>))]
    public async Task<IActionResult> GetTeacherLessons(
        int teacherId, 
        [FromQuery] [SwaggerParameter(Description = "Начальная дата фильтра (например, 2023-05-25T00:00:00)")] DateTime? startDate = null, 
        [FromQuery] [SwaggerParameter(Description = "Конечная дата фильтра (например, 2023-05-30T23:59:59)")] DateTime? endDate = null)
    {
        try 
        {
            _logger.LogInformation("Запрос уроков репетитора с ID: {TeacherId}", teacherId);
            
            // Работаем с датами как с локальными, без преобразования в UTC
            if (startDate.HasValue)
            {
                _logger.LogInformation("Начальная дата фильтра (локальная): {StartDate}", startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                _logger.LogInformation("Конечная дата фильтра (локальная): {EndDate}", endDate.Value);
            }
            
            var lessons = await _teacherService.GetTeacherLessonsAsync(teacherId, startDate, endDate);
            var mappedLessons = _mapper.Map<IEnumerable<LessonDto>>(lessons);
            
            _logger.LogInformation("Возвращается {Count} уроков репетитора", lessons.Count());
            return Ok(mappedLessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении уроков репетитора: {Message}", ex.Message);
            
            // Используем общий обработчик ошибок
            if (ex is ArgumentException)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Пожалуйста, укажите корректную дату", 
                    errorCode = "INVALID_ARGUMENT", 
                    statusCode = 400 
                });
            }
            
            return StatusCode(500, new { 
                success = false, 
                message = "Произошла внутренняя ошибка сервера", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpGet("{teacherId}/lessons/{lessonId}")]
    [SwaggerOperation(Summary = "Получение урока репетитора", Description = "Возвращает данные одного урока репетитора по ID")]
    [SwaggerResponse(200, "Данные урока", typeof(LessonDto))]
    [SwaggerResponse(404, "Урок не найден")]
    public async Task<IActionResult> GetTeacherLesson(int teacherId, int lessonId)
    {
        try
        {
            _logger.LogInformation("Запрос урока с ID: {LessonId} для репетитора с ID: {TeacherId}", lessonId, teacherId);
            
            var lesson = await _teacherService.GetLessonAsync(teacherId, lessonId);
            
            if (lesson == null)
            {
                return NotFound(new { Message = "Урок не найден или не принадлежит данному репетитору" });
            }
            
            var mappedLesson = _mapper.Map<LessonDto>(lesson);
            
            _logger.LogInformation("Возвращается урок с ID: {LessonId}", lessonId);
            return Ok(mappedLesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении урока: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при получении урока", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpGet("{teacherId}/statistics")]
    [SwaggerOperation(Summary = "Получение статистики репетитора", Description = "Возвращает статистическую информацию об уроках, рейтингах и учениках репетитора")]
    [SwaggerResponse(200, "Статистика репетитора", typeof(TeacherStatisticsDto))]
    public async Task<IActionResult> GetTeacherStatistics(int teacherId)
    {
        var statistics = await _teacherService.GetTeacherStatisticsAsync(teacherId);
        var mappedStatistics = _mapper.Map<TeacherStatisticsDto>(statistics);
        return Ok(mappedStatistics);
    }

    [HttpGet("{teacherId}/requests")]
    [SwaggerOperation(Summary = "Получение заявок репетитора", Description = "Возвращает список заявок на обучение от студентов")]
    [SwaggerResponse(200, "Список заявок", typeof(IEnumerable<StudentRequestDto>))]
    public async Task<IActionResult> GetTeacherRequests(int teacherId)
    {
        var requests = await _teacherService.GetTeacherRequestsAsync(teacherId);
        var mappedRequests = _mapper.Map<IEnumerable<StudentRequestDto>>(requests);
        return Ok(mappedRequests);
    }
    
    [HttpPost("accept-request/{requestId}")]
    [SwaggerOperation(Summary = "Принятие заявки от студента", Description = "Принимает заявку на обучение от студента")]
    [SwaggerResponse(200, "Заявка успешно принята")]
    [SwaggerResponse(400, "Ошибка при принятии заявки")]
    public async Task<IActionResult> AcceptStudentRequest(int requestId)
    {
        var result = await _teacherService.AcceptStudentRequestAsync(requestId);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }
    
    [HttpPost("reject-request/{requestId}")]
    [SwaggerOperation(Summary = "Отклонение заявки от студента", Description = "Отклоняет заявку на обучение от студента")]
    [SwaggerResponse(200, "Заявка успешно отклонена")]
    [SwaggerResponse(400, "Ошибка при отклонении заявки")]
    public async Task<IActionResult> RejectStudentRequest(int requestId)
    {
        var result = await _teacherService.RejectStudentRequestAsync(requestId);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }
    
    [HttpDelete("remove-student/{teacherId}/{studentId}")]
    [SwaggerOperation(Summary = "Удаление ученика", Description = "Удаляет ученика у репетитора")]
    [SwaggerResponse(200, "Ученик успешно удален")]
    [SwaggerResponse(400, "Ошибка при удалении ученика")]
    public async Task<IActionResult> RemoveStudent(int teacherId, int studentId)
    {
        _logger.LogInformation("Запрос на удаление ученика: teacherId={TeacherId}, studentId={StudentId}", teacherId, studentId);
        var result = await _teacherService.RemoveStudentAsync(teacherId, studentId);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }
    
    [HttpPost("create-lesson")]
    [SwaggerOperation(Summary = "Создание урока", Description = "Создает новый урок для студента. Поддерживается создание пересекающихся уроков")]
    [SwaggerResponse(200, "Урок успешно создан", typeof(LessonDto))]
    [SwaggerResponse(400, "Ошибка при создании урока")]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request)
    {
        try
        {
            _logger.LogInformation("Запрос на создание урока для учителя {TeacherId} и студента {StudentId}", 
                request.TeacherId, request.StudentId);
                
            // Работаем с датами как с локальными, без преобразования в UTC
            // Это позволит избежать проблем с часовыми поясами
            _logger.LogInformation("Время начала (локальное): {StartTime}", request.StartTime);
            _logger.LogInformation("Время окончания (локальное): {EndTime}", request.EndTime);
            
            // Отключаем преобразование дат в UTC
            var result = await _teacherService.CreateLessonAsync(
                request.TeacherId,
                request.StudentId,
                request.SubjectId,
                request.StartTime,
                request.EndTime,
                request.ConferenceLink,
                request.WhiteboardLink);
            
            if (!result.Success)
            {
                _logger.LogWarning("Ошибка при создании урока: {Message}", result.Message);
                return BadRequest(new { Message = result.Message });
            }
                
            _logger.LogInformation("Урок успешно создан с ID: {LessonId}", result.Lesson.Id);
            return Ok(result.Lesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Ошибка в аргументах при создании урока: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Пожалуйста, укажите корректную дату", 
                errorCode = "INVALID_ARGUMENT", 
                statusCode = 400 
            });
        }
    }
    
    [HttpPost("create-test-completed-lesson")]
    [SwaggerOperation(Summary = "Создание тестового завершенного урока", Description = "Создает новый урок со статусом 'Completed' для тестирования отзывов")]
    [SwaggerResponse(200, "Тестовый урок успешно создан", typeof(LessonDto))]
    [SwaggerResponse(400, "Ошибка при создании тестового урока")]
    public async Task<IActionResult> CreateTestCompletedLesson([FromBody] CreateLessonRequest request)
    {
        try
        {
            _logger.LogInformation("Запрос на создание тестового завершенного урока для учителя {TeacherId} и студента {StudentId}", 
                request.TeacherId, request.StudentId);
                
            // Работаем с датами как с локальными, без преобразования в UTC
            _logger.LogInformation("Время начала (локальное): {StartTime}", request.StartTime);
            _logger.LogInformation("Время окончания (локальное): {EndTime}", request.EndTime);
            
            var result = await _teacherService.CreateLessonAsync(
                request.TeacherId,
                request.StudentId,
                request.SubjectId,
                request.StartTime,
                request.EndTime,
                request.ConferenceLink,
                request.WhiteboardLink);
            
            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }
            
            // Обновляем статус урока на Completed
            var lesson = await _context.Lessons.FindAsync(result.Lesson.Id);
            if (lesson != null)
            {
                lesson.Status = LessonStatus.Completed;
                await _context.SaveChangesAsync();
                
                // Обновляем DTO
                result.Lesson.Status = LessonStatus.Completed.ToString();
            }
                
            _logger.LogInformation("Тестовый завершенный урок успешно создан с ID: {LessonId}", result.Lesson.Id);
            return Ok(result.Lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании тестового урока: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при создании тестового урока", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }
    
    [HttpPost("cancel-lesson/{teacherId}/{lessonId}")]
    [SwaggerOperation(Summary = "Отмена урока", Description = "Отменяет запланированный урок")]
    [SwaggerResponse(200, "Урок успешно отменен")]
    [SwaggerResponse(400, "Ошибка при отмене урока")]
    public async Task<IActionResult> CancelLesson(int teacherId, int lessonId)
    {
        _logger.LogInformation("Запрос на отмену урока с ID: {LessonId} от репетитора с ID: {TeacherId}", lessonId, teacherId);
        
        var result = await _teacherService.CancelLessonAsync(teacherId, lessonId);
        
        if (!result.Success)
        {
            _logger.LogWarning("Ошибка при отмене урока: {Message}", result.Message);
            return BadRequest(new { Message = result.Message });
        }
        
        _logger.LogInformation("Урок с ID: {LessonId} успешно отменен", lessonId);
        return Ok(new { Message = result.Message });
    }

    [HttpPut("update-lesson-status/{teacherId}/{lessonId}")]
    [SwaggerOperation(Summary = "Обновление статуса урока", Description = "Обновляет статус урока вручную")]
    [SwaggerResponse(200, "Статус урока успешно обновлен")]
    [SwaggerResponse(400, "Ошибка при обновлении статуса урока")]
    [SwaggerResponse(404, "Урок не найден")]
    public async Task<IActionResult> UpdateLessonStatus(int teacherId, int lessonId, [FromQuery] LessonStatus status)
    {
        _logger.LogInformation("Запрос на обновление статуса урока с ID: {LessonId} на {Status} от репетитора с ID: {TeacherId}", 
            lessonId, status, teacherId);
            
        try
        {
            // Находим урок
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.TeacherId == teacherId);
                
            if (lesson == null)
            {
                _logger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит репетитору с ID={TeacherId}", lessonId, teacherId);
                return NotFound(new { Message = "Урок не найден или не принадлежит данному репетитору" });
            }
            
            // Обновляем статус урока
            lesson.Status = status;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Статус урока с ID={LessonId} успешно обновлен на {Status}", lessonId, status);
            return Ok(new { 
                Message = $"Статус урока успешно обновлен на {status}", 
                LessonId = lessonId,
                Status = status.ToString() 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса урока: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при обновлении статуса урока", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpGet("lesson/{lessonId}/attachments")]
    [SwaggerOperation(Summary = "Получение вложений урока", Description = "Возвращает список файлов, прикрепленных к уроку")]
    [SwaggerResponse(200, "Список вложений", typeof(IEnumerable<AttachmentDto>))]
    public async Task<IActionResult> GetLessonAttachments(int lessonId)
    {
        var attachments = await _teacherService.GetLessonAttachmentsAsync(lessonId);
        var mappedAttachments = _mapper.Map<IEnumerable<AttachmentDto>>(attachments);
        return Ok(mappedAttachments);
    }

    [HttpPost("lesson/{lessonId}/upload-attachment")]
    [SwaggerOperation(Summary = "Загрузка файла к уроку", Description = "Загружает новый файл и прикрепляет к указанному уроку")]
    [SwaggerResponse(200, "Файл успешно загружен")]
    [SwaggerResponse(400, "Ошибка при загрузке файла")]
    public async Task<IActionResult> UploadLessonAttachment(
        int teacherId, 
        int lessonId, 
        [FromBody] UploadAttachmentRequest request)
    {
        var result = await _teacherService.UploadLessonAttachmentAsync(
            teacherId,
            lessonId,
            request.FileName,
            request.FileType,
            request.Base64Content);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }

    [HttpGet("{teacherId}/reviews")]
    [SwaggerOperation(Summary = "Получение отзывов репетитора", Description = "Возвращает список отзывов, оставленных студентами о репетиторе")]
    [SwaggerResponse(200, "Список отзывов", typeof(IEnumerable<ReviewDto>))]
    public async Task<IActionResult> GetTeacherReviews(int teacherId)
    {
        // Используем сервис отзывов для получения данных
        var reviewService = HttpContext.RequestServices.GetRequiredService<ReviewService>();
        var reviews = await reviewService.GetTeacherReviewsAsync(teacherId);
        var mappedReviews = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        return Ok(mappedReviews);
    }

    [HttpGet("{teacherId}/rating")]
    [SwaggerOperation(Summary = "Получение рейтинга репетитора", Description = "Возвращает текущий рейтинг и количество отзывов о репетиторе")]
    [SwaggerResponse(200, "Информация о рейтинге репетитора")]
    [SwaggerResponse(404, "Репетитор не найден")]
    public async Task<IActionResult> GetTeacherRating(int teacherId)
    {
        try
        {
            var teacherProfile = await _context.TeacherProfiles
                .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
                
            if (teacherProfile == null)
            {
                _logger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
                return NotFound(new { Message = "Репетитор не найден" });
            }
            
            // Получаем количество отзывов
            var reviewsCount = await _context.Reviews
                .Where(r => r.TeacherId == teacherId)
                .CountAsync();
                
            return Ok(new { 
                TeacherId = teacherId,
                TeacherProfileId = teacherProfile.Id,
                Rating = teacherProfile.Rating,
                ReviewsCount = teacherProfile.ReviewsCount,
                ActualReviewsCount = reviewsCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рейтинга репетитора: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при получении рейтинга репетитора", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpPost("{teacherId}/update-rating")]
    [SwaggerOperation(Summary = "Обновление рейтинга репетитора", Description = "Принудительно пересчитывает рейтинг репетитора на основе всех отзывов")]
    [SwaggerResponse(200, "Рейтинг успешно обновлен")]
    [SwaggerResponse(404, "Репетитор не найден")]
    public async Task<IActionResult> UpdateTeacherRating(int teacherId)
    {
        try
        {
            var teacherProfile = await _context.TeacherProfiles
                .FirstOrDefaultAsync(tp => tp.UserId == teacherId);
                
            if (teacherProfile == null)
            {
                _logger.LogWarning("Профиль репетитора с UserId={TeacherId} не найден", teacherId);
                return NotFound(new { Message = "Репетитор не найден" });
            }
            
            // Получаем все отзывы о репетиторе
            var reviews = await _context.Reviews
                .Where(r => r.TeacherId == teacherId)
                .ToListAsync();
                
            double rating = 0;
            if (reviews.Any())
            {
                rating = reviews.Average(r => r.Rating);
            }
            
            // Обновляем рейтинг и количество отзывов
            teacherProfile.Rating = rating;
            teacherProfile.ReviewsCount = reviews.Count;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Рейтинг репетитора с ID={TeacherId} успешно обновлен: {Rating} на основе {Count} отзывов", 
                teacherId, rating, reviews.Count);
                
            return Ok(new { 
                Message = "Рейтинг успешно обновлен",
                TeacherId = teacherId,
                Rating = rating,
                ReviewsCount = reviews.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении рейтинга репетитора: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при обновлении рейтинга репетитора", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpGet("student-profile/{studentId}")]
    [SwaggerOperation(Summary = "Получение профиля студента", Description = "Возвращает данные профиля студента по ID")]
    [SwaggerResponse(200, "Профиль студента", typeof(StudentProfileDto))]
    [SwaggerResponse(404, "Студент не найден")]
    public async Task<IActionResult> GetStudentProfile(int studentId)
    {
        _logger.LogInformation("Получен запрос профиля студента с ID: {StudentId} от преподавателя", studentId);
        
        var studentService = HttpContext.RequestServices.GetRequiredService<StudentService>();
        var profile = await studentService.GetStudentProfileAsync(studentId);
        
        if (profile == null)
        {
            _logger.LogWarning("Профиль студента с ID: {StudentId} не найден", studentId);
            return NotFound();
        }
            
        var mappedProfile = _mapper.Map<StudentProfileDto>(profile);
        
        // Добавляем роли в UserDto, если он существует
        if (mappedProfile.User != null)
        {
            mappedProfile.User.IsTeacher = false;
            mappedProfile.User.Roles = new List<string> { "STUDENT" };
        }
        
        return Ok(mappedProfile);
    }

    /// <summary>
    /// Обновляет статусы всех уроков на основе текущего времени
    /// </summary>
    /// <returns>Количество обновленных уроков</returns>
    [HttpPost("lessons/update-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAllLessonsStatus()
    {
        _logger.LogInformation("Вызов API для обновления статусов всех уроков");
        
        var updatedCount = await _teacherService.UpdateAllLessonsStatusAsync();
        
        return Ok(new { 
            success = true, 
            message = $"Обновлено {updatedCount} уроков", 
            updatedCount 
        });
    }
} 