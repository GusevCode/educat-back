using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Vibetech.Educat.API.Models;

public class LoginRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class StudentRegisterRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
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

public class TeacherRegisterRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    public string MiddleName { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Format = "date", Description = "Дата рождения в формате YYYY-MM-DD")]
    public DateTime BirthDate { get; set; }
    
    [Required]
    public string Gender { get; set; } = string.Empty;
    
    [Required]
    public string ContactInfo { get; set; } = string.Empty;
    
    public string PhotoBase64 { get; set; } = string.Empty;
    
    [Required]
    public string Education { get; set; } = string.Empty;
    
    public List<int> PreparationProgramIds { get; set; } = new();
    
    [Required]
    public int ExperienceYears { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }
    
    public List<int> SubjectIds { get; set; } = new();
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    
    [SwaggerSchema(Format = "date", Description = "Дата рождения в формате YYYY-MM-DD")]
    public DateTime BirthDate { get; set; }
    
    public string Gender { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public bool IsTeacher { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? PhotoBase64 { get; set; }
} 