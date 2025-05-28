using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Core.DTOs;

/// <summary>
/// DTO для входа в систему
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    [Required(ErrorMessage = "Логин обязателен")]
    public string Login { get; set; } = string.Empty;
    
    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO для результата аутентификации
/// </summary>
public class AuthResultDto
{
    /// <summary>
    /// Токен доступа (JWT)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Токен обновления
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Срок действия токена доступа
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public UserDto User { get; set; } = new();
}

/// <summary>
/// DTO для базовой информации о пользователе
/// </summary>
public class UserDto
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Отчество пользователя
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата рождения пользователя
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// Пол пользователя
    /// </summary>
    public string Gender { get; set; } = string.Empty;
    
    /// <summary>
    /// Контактная информация пользователя
    /// </summary>
    public string ContactInfo { get; set; } = string.Empty;
    
    /// <summary>
    /// Признак преподавателя
    /// </summary>
    public bool IsTeacher { get; set; }
    
    /// <summary>
    /// Роли пользователя
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Фотография пользователя в формате Base64
    /// </summary>
    public string? PhotoBase64 { get; set; }
} 