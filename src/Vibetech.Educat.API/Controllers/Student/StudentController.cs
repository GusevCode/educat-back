using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vibetech.Educat.Services.Services;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.API.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using System.Collections.Generic;

namespace Vibetech.Educat.API.Controllers.Student;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("API для студентов")]
public class StudentController : ControllerBase
{
    private readonly StudentService _studentService;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentController> _logger;
    private readonly EducatDbContext _context;

    public StudentController(StudentService studentService, IMapper mapper, ILogger<StudentController> logger, EducatDbContext context)
    {
        _studentService = studentService;
        _mapper = mapper;
        _logger = logger;
        _context = context;
    }

    [HttpGet("profile/{studentId}")]
    [SwaggerOperation(Summary = "Получение профиля студента", Description = "Возвращает данные профиля студента по ID")]
    [SwaggerResponse(200, "Профиль студента", typeof(StudentProfileDto))]
    [SwaggerResponse(404, "Студент не найден")]
    public async Task<IActionResult> GetStudentProfile(int studentId)
    {
        var profile = await _studentService.GetStudentProfileAsync(studentId);
        
        if (profile == null)
            return NotFound();
            
        var mappedProfile = _mapper.Map<StudentProfileDto>(profile);
        
        // Добавляем роли в UserDto, если он существует
        if (mappedProfile.User != null)
        {
            mappedProfile.User.IsTeacher = false;
            mappedProfile.User.Roles = new List<string> { "STUDENT" };
        }
        
        return Ok(mappedProfile);
    }

    [HttpPut("profile/{studentId}")]
    [SwaggerOperation(Summary = "Обновление профиля студента", Description = "Обновляет данные профиля студента")]
    [SwaggerResponse(200, "Профиль студента успешно обновлен", typeof(StudentProfileDto))]
    [SwaggerResponse(404, "Студент не найден")]
    public async Task<IActionResult> UpdateStudentProfile(int studentId, [FromBody] UpdateStudentProfileRequest request)
    {
        var profile = await _studentService.UpdateStudentProfileAsync(studentId, 
            request.FirstName, request.LastName, request.MiddleName,
            request.BirthDate, request.Gender, request.ContactInfo, request.PhotoBase64);
        
        if (profile == null)
            return NotFound();
            
        var mappedProfile = _mapper.Map<StudentProfileDto>(profile);
        return Ok(mappedProfile);
    }

    [HttpGet("{studentId}/teachers")]
    [SwaggerOperation(Summary = "Получение репетиторов студента", Description = "Возвращает список идентификаторов пользователей (UserId) репетиторов для указанного студента")]
    [SwaggerResponse(200, "Список идентификаторов пользователей репетиторов", typeof(IEnumerable<int>))]
    public async Task<IActionResult> GetStudentTeachers(int studentId)
    {
        var teacherIds = await _studentService.GetStudentTeachersAsync(studentId);
        return Ok(teacherIds);
    }

    [HttpGet("{studentId}/lessons")]
    [SwaggerOperation(Summary = "Получение уроков студента", Description = "Возвращает список уроков студента с возможностью фильтрации по датам")]
    [SwaggerResponse(200, "Список уроков студента", typeof(IEnumerable<LessonDto>))]
    public async Task<IActionResult> GetStudentLessons(
        int studentId, 
        [FromQuery] [SwaggerParameter(Description = "Начальная дата фильтра (например, 2023-05-25T00:00:00)")] DateTime? startDate = null, 
        [FromQuery] [SwaggerParameter(Description = "Конечная дата фильтра (например, 2023-05-30T23:59:59)")] DateTime? endDate = null)
    {
        try 
        {
            _logger.LogInformation("Запрос уроков студента с ID: {StudentId}", studentId);
            
            // Работаем с датами как с локальными, без преобразования в UTC
            if (startDate.HasValue)
            {
                _logger.LogInformation("Начальная дата фильтра (локальная): {StartDate}", startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                _logger.LogInformation("Конечная дата фильтра (локальная): {EndDate}", endDate.Value);
            }
            
            var lessons = await _studentService.GetStudentLessonsAsync(studentId, startDate, endDate);
            var mappedLessons = _mapper.Map<IEnumerable<LessonDto>>(lessons);
            
            _logger.LogInformation("Возвращается {Count} уроков студента", lessons.Count());
            return Ok(mappedLessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении уроков студента: {Message}", ex.Message);
            
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

    [HttpGet("{studentId}/lessons/{lessonId}")]
    [SwaggerOperation(Summary = "Получение урока студента", Description = "Возвращает данные одного урока студента по ID")]
    [SwaggerResponse(200, "Данные урока", typeof(LessonDto))]
    [SwaggerResponse(404, "Урок не найден")]
    public async Task<IActionResult> GetStudentLesson(int studentId, int lessonId)
    {
        try
        {
            _logger.LogInformation("Запрос урока с ID: {LessonId} для студента с ID: {StudentId}", lessonId, studentId);
            
            var lesson = await _studentService.GetLessonAsync(studentId, lessonId);
            
            if (lesson == null)
            {
                return NotFound(new { Message = "Урок не найден или не принадлежит данному студенту" });
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

    [HttpGet("{studentId}/statistics")]
    [SwaggerOperation(Summary = "Получение статистики студента", Description = "Возвращает статистическую информацию об уроках и репетиторах студента")]
    [SwaggerResponse(200, "Статистика студента", typeof(StudentStatisticsDto))]
    public async Task<IActionResult> GetStudentStatistics(int studentId)
    {
        var statistics = await _studentService.GetStudentStatisticsAsync(studentId);
        var mappedStatistics = _mapper.Map<StudentStatisticsDto>(statistics);
        return Ok(mappedStatistics);
    }

    [HttpGet("search-tutors")]
    [SwaggerOperation(Summary = "Поиск репетиторов", Description = "Поиск репетиторов с различными фильтрами")]
    [SwaggerResponse(200, "Список репетиторов", typeof(IEnumerable<TeacherProfileDto>))]
    public async Task<IActionResult> SearchTutors(
        [FromQuery] int? subjectId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minExperience = null,
        [FromQuery] double? minRating = null)
    {
        var tutors = await _studentService.SearchTutorsAsync(
            subjectId, minPrice, maxPrice, minExperience, minRating);
            
        var mappedTutors = _mapper.Map<IEnumerable<TeacherProfileDto>>(tutors);
        return Ok(mappedTutors);
    }

    [HttpPost("send-request/{teacherId}")]
    [SwaggerOperation(Summary = "Отправка заявки репетитору", Description = "Отправляет заявку на обучение выбранному репетитору")]
    [SwaggerResponse(200, "Заявка успешно отправлена")]
    [SwaggerResponse(400, "Ошибка при отправке заявки")]
    public async Task<IActionResult> SendRequestToTeacher(int studentId, int teacherId)
    {
        var result = await _studentService.SendRequestToTeacherAsync(studentId, teacherId);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }

    [HttpGet("lesson/{lessonId}/attachments")]
    [SwaggerOperation(Summary = "Получение вложений урока", Description = "Возвращает список файлов, прикрепленных к уроку")]
    [SwaggerResponse(200, "Список вложений", typeof(IEnumerable<AttachmentDto>))]
    public async Task<IActionResult> GetLessonAttachments(int lessonId)
    {
        var attachments = await _studentService.GetLessonAttachmentsAsync(lessonId);
        var mappedAttachments = _mapper.Map<IEnumerable<AttachmentDto>>(attachments);
        return Ok(mappedAttachments);
    }

    [HttpPost("lesson/{lessonId}/upload-attachment")]
    [SwaggerOperation(Summary = "Загрузка файла к уроку", Description = "Загружает новый файл и прикрепляет к указанному уроку")]
    [SwaggerResponse(200, "Файл успешно загружен")]
    [SwaggerResponse(400, "Ошибка при загрузке файла")]
    public async Task<IActionResult> UploadLessonAttachment(
        int studentId, 
        int lessonId, 
        [FromBody] UploadAttachmentRequest request)
    {
        var result = await _studentService.UploadLessonAttachmentAsync(
            studentId,
            lessonId,
            request.FileName,
            request.FileType,
            request.Base64Content);
        
        if (!result.Success)
            return BadRequest(new { Message = result.Message });
            
        return Ok(new { Message = result.Message });
    }

    [HttpPost("cancel-lesson/{studentId}/{lessonId}")]
    [SwaggerOperation(Summary = "Отмена урока", Description = "Отменяет запланированный урок")]
    [SwaggerResponse(200, "Урок успешно отменен")]
    [SwaggerResponse(400, "Ошибка при отмене урока")]
    public async Task<IActionResult> CancelLesson(int studentId, int lessonId)
    {
        _logger.LogInformation("Запрос на отмену урока с ID: {LessonId} от студента с ID: {StudentId}", lessonId, studentId);
        
        var result = await _studentService.CancelLessonAsync(studentId, lessonId);
        
        if (!result.Success)
        {
            _logger.LogWarning("Ошибка при отмене урока: {Message}", result.Message);
            return BadRequest(new { Message = result.Message });
        }
        
        _logger.LogInformation("Урок с ID: {LessonId} успешно отменен", lessonId);
        return Ok(new { Message = result.Message });
    }

    [HttpPost("force-complete-lesson/{studentId}/{lessonId}")]
    [SwaggerOperation(Summary = "Принудительное завершение урока", Description = "Меняет статус урока на 'Completed' для тестирования отзывов")]
    [SwaggerResponse(200, "Статус урока успешно обновлен")]
    [SwaggerResponse(400, "Ошибка при обновлении статуса урока")]
    [SwaggerResponse(404, "Урок не найден")]
    public async Task<IActionResult> ForceCompleteLesson(int studentId, int lessonId)
    {
        _logger.LogInformation("Запрос на принудительное завершение урока с ID: {LessonId} от студента с ID: {StudentId}", 
            lessonId, studentId);
            
        try
        {
            // Проверяем существование студента
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
                
            if (student == null)
            {
                _logger.LogWarning("Студент с ID={StudentId} не найден", studentId);
                return BadRequest(new { Message = "Студент не найден" });
            }
            
            // Находим урок
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.StudentId == studentId);
                
            if (lesson == null)
            {
                _logger.LogWarning("Урок с ID={LessonId} не найден или не принадлежит студенту с ID={StudentId}", lessonId, studentId);
                return NotFound(new { Message = "Урок не найден или не принадлежит данному студенту" });
            }
            
            // Обновляем статус урока на Completed
            lesson.Status = LessonStatus.Completed;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Статус урока с ID={LessonId} успешно обновлен на Completed", lessonId);
            return Ok(new { 
                Message = "Статус урока успешно обновлен на Completed", 
                LessonId = lessonId,
                Status = "Completed" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при принудительном завершении урока: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при принудительном завершении урока", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }

    [HttpGet("{studentId}/reviews")]
    [SwaggerOperation(Summary = "Получение отзывов, оставленных студентом", Description = "Возвращает список отзывов, оставленных студентом о репетиторах")]
    [SwaggerResponse(200, "Список отзывов", typeof(IEnumerable<ReviewDto>))]
    public async Task<IActionResult> GetStudentReviews(int studentId)
    {
        // Проверяем, что текущий пользователь запрашивает свои отзывы
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || int.Parse(userIdClaim) != studentId)
        {
            return Forbid();
        }
        
        // Используем сервис отзывов для получения данных
        var reviewService = HttpContext.RequestServices.GetRequiredService<ReviewService>();
        var reviews = await reviewService.GetStudentReviewsAsync(studentId);
        var mappedReviews = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        return Ok(mappedReviews);
    }

    [HttpPost("review")]
    [SwaggerOperation(Summary = "Создание отзыва о репетиторе", Description = "Создает новый отзыв студента о репетиторе для указанного урока")]
    [SwaggerResponse(200, "Отзыв успешно создан", typeof(ReviewDto))]
    [SwaggerResponse(400, "Ошибка при создании отзыва")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        // Проверяем, что текущий пользователь создает отзыв от своего имени
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || int.Parse(userIdClaim) != request.StudentId)
        {
            return Forbid();
        }
        
        // Используем сервис отзывов для создания отзыва
        var reviewService = HttpContext.RequestServices.GetRequiredService<ReviewService>();
        var result = await reviewService.CreateReviewAsync(
            request.LessonId, 
            request.TeacherId, 
            request.StudentId, 
            request.Rating, 
            request.Comment);
            
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }
        
        var mappedReview = _mapper.Map<ReviewDto>(result.Review);
        
        // Получаем обновленную информацию о рейтинге репетитора
        var teacherProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == request.TeacherId);
            
        return Ok(new { 
            Message = result.Message, 
            Review = mappedReview,
            TeacherRating = teacherProfile?.Rating ?? 0,
            ReviewsCount = teacherProfile?.ReviewsCount ?? 0
        });
    }

    [HttpGet("teacher-profile/{teacherId}")]
    [SwaggerOperation(Summary = "Получение профиля преподавателя", Description = "Возвращает данные профиля преподавателя по ID")]
    [SwaggerResponse(200, "Профиль преподавателя", typeof(TeacherProfileDto))]
    [SwaggerResponse(404, "Преподаватель не найден")]
    public async Task<IActionResult> GetTeacherProfile(int teacherId)
    {
        _logger.LogInformation("Получен запрос профиля преподавателя с ID: {TeacherId} от студента", teacherId);
        
        var teacherService = HttpContext.RequestServices.GetRequiredService<TeacherService>();
        var profile = await teacherService.GetTeacherProfileAsync(teacherId);
        
        if (profile == null)
        {
            _logger.LogWarning("Профиль преподавателя с ID: {TeacherId} не найден", teacherId);
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
} 