using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Vibetech.Educat.API.Models;

public class TeacherProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal HourlyRate { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public List<PreparationProgramDto> PreparationPrograms { get; set; } = new();
    public List<SubjectDto> Subjects { get; set; } = new();
    
    // Вложенный объект User для полной информации о пользователе
    public UserDto? User { get; set; }
}

public class UpdateTeacherProfileRequest
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
    
    [Required]
    public string Education { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 50)]
    public int ExperienceYears { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }
    
    public List<int> SubjectIds { get; set; } = new();
    
    public List<int> PreparationProgramIds { get; set; } = new();
    
    public string PhotoBase64 { get; set; } = string.Empty;
}

public class PreparationProgramDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TeacherStudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    [SwaggerSchema(Format = "date", Description = "Дата принятия заявки в формате YYYY-MM-DD")]
    public DateTime AcceptedDate { get; set; }
    
    public string ContactInfo { get; set; } = string.Empty;
}

public class TeacherStatisticsDto
{
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int UpcomingLessons { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public Dictionary<int, int> LessonsBySubject { get; set; } = new(); // SubjectId -> Count
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Rating -> Count
} 