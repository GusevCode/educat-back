using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Vibetech.Educat.API.Models;
using Vibetech.Educat.Services.Services;
using Vibetech.Educat.DataAccess.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Vibetech.Educat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Аутентификация и авторизация пользователей")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly UserManager<User> _userManager;

    public AuthController(AuthService authService, UserManager<User> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [SwaggerOperation(Summary = "Авторизация пользователя", Description = "Авторизует пользователя в системе и возвращает JWT-токен")]
    [SwaggerResponse(200, "Пользователь успешно авторизован", typeof(AuthResponse))]
    [SwaggerResponse(400, "Неверные учетные данные")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, message, token, user) = await _authService.LoginAsync(request.Login, request.Password);
        
        if (!success || user == null)
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = message
            });
            
        var roles = await _userManager.GetRolesAsync(user);
        
        return Ok(new AuthResponse
        {
            Success = true,
            Message = message,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                ContactInfo = user.ContactInformation,
                IsTeacher = user.Role == "Teacher",
                Roles = roles.ToList(),
                PhotoBase64 = user.PhotoBase64
            }
        });
    }

    [HttpPost("register/student")]
    [SwaggerOperation(Summary = "Регистрация студента", Description = "Регистрирует нового студента в системе")]
    [SwaggerResponse(200, "Студент успешно зарегистрирован", typeof(AuthResponse))]
    [SwaggerResponse(400, "Ошибка регистрации")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Пароли не совпадают"
            });
            
        var (success, message, user) = await _authService.RegisterStudentAsync(
            request.Login,
            request.Password,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.BirthDate,
            request.Gender,
            request.ContactInfo,
            request.PhotoBase64);
            
        if (!success)
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = message
            });
            
        var roles = await _userManager.GetRolesAsync(user);
        
        return Ok(new AuthResponse
        {
            Success = true,
            Message = message,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                ContactInfo = user.ContactInformation,
                IsTeacher = user.Role == "Teacher",
                Roles = roles.ToList(),
                PhotoBase64 = user.PhotoBase64
            }
        });
    }

    [HttpPost("register/teacher")]
    [SwaggerOperation(Summary = "Регистрация преподавателя", Description = "Регистрирует нового преподавателя в системе. Поля SubjectIds и PreparationProgramIds не обязательны и могут быть добавлены позже.")]
    [SwaggerResponse(200, "Преподаватель успешно зарегистрирован", typeof(AuthResponse))]
    [SwaggerResponse(400, "Ошибка регистрации")]
    public async Task<IActionResult> RegisterTeacher([FromBody] TeacherRegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Пароли не совпадают"
            });

        var (success, message, user, teacherProfile) = await _authService.RegisterTeacherAsync(
            request.Login,
            request.Password,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.BirthDate,
            request.Gender,
            request.ContactInfo,
            request.Education,
            request.ExperienceYears,
            request.HourlyRate,
            request.SubjectIds,
            request.PreparationProgramIds,
            request.PhotoBase64);

        if (!success)
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = message
            });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = message,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                ContactInfo = user.ContactInformation,
                IsTeacher = user.Role == "Teacher",
                Roles = roles.ToList(),
                PhotoBase64 = user.PhotoBase64
            }
        });
    }
} 