using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibetech.Educat.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("API для администраторов")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly EducatDbContext _context;

    public AdminController(ILogger<AdminController> logger, EducatDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("update-all-ratings")]
    [SwaggerOperation(Summary = "Обновление всех рейтингов", Description = "Обновляет рейтинги всех репетиторов на основе отзывов")]
    [SwaggerResponse(200, "Все рейтинги успешно обновлены")]
    public async Task<IActionResult> UpdateAllRatings()
    {
        try
        {
            _logger.LogInformation("Запрос на обновление всех рейтингов репетиторов");
            
            // Получаем всех репетиторов
            var teacherProfiles = await _context.TeacherProfiles
                .ToListAsync();
                
            _logger.LogInformation("Найдено {Count} профилей репетиторов для обновления рейтинга", teacherProfiles.Count);
            
            var updatedTeachers = new List<object>();
            
            // Обновляем рейтинг для каждого репетитора
            foreach (var teacherProfile in teacherProfiles)
            {
                // Получаем все отзывы о репетиторе
                var reviews = await _context.Reviews
                    .Where(r => r.TeacherId == teacherProfile.UserId)
                    .ToListAsync();
                    
                double rating = 0;
                if (reviews.Any())
                {
                    rating = reviews.Average(r => r.Rating);
                }
                
                // Обновляем рейтинг и количество отзывов
                teacherProfile.Rating = rating;
                teacherProfile.ReviewsCount = reviews.Count;
                
                updatedTeachers.Add(new { 
                    TeacherId = teacherProfile.UserId,
                    ProfileId = teacherProfile.Id,
                    Rating = rating,
                    ReviewsCount = reviews.Count
                });
                
                _logger.LogInformation("Обновлен рейтинг репетитора UserId={TeacherId}, ProfileId={ProfileId}: {Rating} на основе {Count} отзывов", 
                    teacherProfile.UserId, teacherProfile.Id, rating, reviews.Count);
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Все рейтинги успешно обновлены");
                
            return Ok(new { 
                Message = "Все рейтинги успешно обновлены",
                UpdatedTeachers = updatedTeachers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении всех рейтингов: {Message}", ex.Message);
            return BadRequest(new { 
                success = false, 
                message = "Ошибка при обновлении всех рейтингов", 
                errorCode = "INTERNAL_ERROR", 
                statusCode = 500 
            });
        }
    }
} 