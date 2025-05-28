using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vibetech.Educat.Services.Services;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.API.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Vibetech.Educat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("API для работы с отзывами")]
public class ReviewController : ControllerBase
{
    private readonly ReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;
    private readonly IMapper _mapper;

    public ReviewController(ReviewService reviewService, ILogger<ReviewController> logger, IMapper mapper)
    {
        _reviewService = reviewService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("teacher/{teacherId}")]
    [SwaggerOperation(Summary = "Получение отзывов репетитора", 
        Description = "Возвращает список отзывов, оставленных студентами для указанного репетитора")]
    [SwaggerResponse(200, "Список отзывов", typeof(IEnumerable<ReviewDto>))]
    public async Task<IActionResult> GetTeacherReviews(int teacherId)
    {
        var reviews = await _reviewService.GetTeacherReviewsAsync(teacherId);
        var mappedReviews = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        return Ok(mappedReviews);
    }

    [HttpGet("lesson/{lessonId}")]
    [SwaggerOperation(Summary = "Получение отзывов об уроке", 
        Description = "Возвращает список отзывов, оставленных для указанного урока")]
    [SwaggerResponse(200, "Список отзывов", typeof(IEnumerable<ReviewDto>))]
    public async Task<IActionResult> GetLessonReviews(int lessonId)
    {
        var reviews = await _reviewService.GetLessonReviewsAsync(lessonId);
        var mappedReviews = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        return Ok(mappedReviews);
    }

    [HttpGet("student/{studentId}")]
    [SwaggerOperation(Summary = "Получение отзывов, оставленных студентом", 
        Description = "Возвращает список отзывов, оставленных указанным студентом")]
    [SwaggerResponse(200, "Список отзывов", typeof(IEnumerable<ReviewDto>))]
    public async Task<IActionResult> GetStudentReviews(int studentId)
    {
        // В режиме разработки пропускаем проверку авторизации
        var reviews = await _reviewService.GetStudentReviewsAsync(studentId);
        var mappedReviews = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        return Ok(mappedReviews);
    }

    [HttpPost("create")]
    [SwaggerOperation(Summary = "Создание отзыва", 
        Description = "Создает новый отзыв о репетиторе для указанного урока")]
    [SwaggerResponse(200, "Отзыв успешно создан", typeof(ReviewDto))]
    [SwaggerResponse(400, "Ошибка при создании отзыва")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            // В режиме разработки пропускаем проверку авторизации
            var result = await _reviewService.CreateReviewAsync(
                request.LessonId, 
                request.TeacherId, 
                request.StudentId, 
                request.Rating, 
                request.Comment);
                
            if (!result.Success)
            {
                _logger.LogWarning("Ошибка при создании отзыва: {Message}", result.Message);
                return BadRequest(new { Message = result.Message });
            }
            
            var mappedReview = _mapper.Map<ReviewDto>(result.Review);
            return Ok(new { 
                Message = result.Message, 
                Review = mappedReview 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка при создании отзыва");
            return StatusCode(500, new { Message = "Внутренняя ошибка сервера при создании отзыва" });
        }
    }
} 