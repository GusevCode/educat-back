using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Vibetech.Educat.API.Models;

public class ReviewDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    
    [SwaggerSchema(Format = "date-time", Description = "Дата создания отзыва")]
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    [Required]
    public int LessonId { get; set; }
    
    [Required]
    public int TeacherId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Комментарий должен содержать от 10 до 2000 символов")]
    public string Comment { get; set; } = string.Empty;
} 