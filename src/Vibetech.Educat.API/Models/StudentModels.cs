using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Vibetech.Educat.API.Models;

public class StudentProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    
    // Вложенный объект User для полной информации о пользователе
    public UserDto? User { get; set; }
}

public class UpdateStudentProfileRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string MiddleName { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Format = "date", Description = "Дата рождения в формате YYYY-MM-DD")]
    public DateTime BirthDate { get; set; }
    
    [Required]
    public string Gender { get; set; } = string.Empty;
    
    [Required]
    public string ContactInfo { get; set; } = string.Empty;
    
    public string PhotoBase64 { get; set; } = string.Empty;
}

public class StudentStatisticsDto
{
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int UpcomingLessons { get; set; }
    public int TeachersCount { get; set; }
    public Dictionary<int, int> LessonsBySubject { get; set; } = new(); // SubjectId -> Count
    public int TotalLessonHours { get; set; }
}

public class StudentTeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    
    [SwaggerSchema(Format = "date", Description = "Дата принятия заявки в формате YYYY-MM-DD")]
    public DateTime AcceptedDate { get; set; }
}

public class StudentRequestDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    
    [SwaggerSchema(Format = "date", Description = "Дата запроса в формате YYYY-MM-DD")]
    public DateTime RequestDate { get; set; }
    
    public string Status { get; set; } = string.Empty;
}

public class TutorSearchRequest
{
    public int? SubjectId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinExperience { get; set; }
    public double? MinRating { get; set; }
}

public class ApplicationRequestDto
{
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public string Message { get; set; } = string.Empty;
} 