using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.Services.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly EducatDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        EducatDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterStudentAsync(
        string login,
        string password,
        string lastName,
        string firstName,
        string middleName,
        DateTime birthDate,
        string gender,
        string contactInfo,
        string photoBase64 = null)
    {
        var userExists = await _userManager.FindByNameAsync(login);
        if (userExists != null)
            return (false, "Пользователь с таким логином уже существует", null);

        // Убедимся, что дата рождения имеет тип DateTimeKind.Utc
        birthDate = new DateTime(birthDate.Year, birthDate.Month, birthDate.Day, 12, 0, 0, DateTimeKind.Utc);

        var user = new User
        {
            UserName = login,
            Email = login + "@example.com", // Фиктивный email для совместимости
            LastName = lastName,
            FirstName = firstName,
            MiddleName = middleName,
            BirthDate = birthDate,
            Gender = gender,
            ContactInformation = contactInfo,
            PhotoBase64 = photoBase64,
            Role = "Student",
            Login = login,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

        await _userManager.AddToRoleAsync(user, "Student");
        return (true, "Регистрация студента успешно завершена", user);
    }

    public async Task<(bool Success, string Message, User? User, TeacherProfile? TeacherProfile)> RegisterTeacherAsync(
        string login,
        string password,
        string lastName,
        string firstName,
        string middleName,
        DateTime birthDate,
        string gender,
        string contactInfo,
        string education,
        int experienceYears,
        decimal hourlyRate,
        List<int> subjectIds = null,
        List<int> preparationProgramIds = null,
        string photoBase64 = null)
    {
        var userExists = await _userManager.FindByNameAsync(login);
        if (userExists != null)
            return (false, "Пользователь с таким логином уже существует", null, null);

        try
        {
            // Убедимся, что дата рождения имеет тип DateTimeKind.Utc
            birthDate = new DateTime(birthDate.Year, birthDate.Month, birthDate.Day, 12, 0, 0, DateTimeKind.Utc);
            
            var user = new User
            {
                UserName = login,
                Email = login + "@example.com", // Фиктивный email для совместимости
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                BirthDate = birthDate,
                Gender = gender,
                ContactInformation = contactInfo,
                PhotoBase64 = photoBase64,
                Role = "Teacher",
                Login = login,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null, null);

            await _userManager.AddToRoleAsync(user, "Teacher");

            // Создаем профиль репетитора
            var teacherProfile = new TeacherProfile
            {
                UserId = user.Id,
                Education = education,
                ExperienceYears = experienceYears,
                HourlyRate = hourlyRate,
                Rating = 0,
                ReviewsCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _context.TeacherProfiles.AddAsync(teacherProfile);
            await _context.SaveChangesAsync();

            // Добавляем предметы репетитора
            if (subjectIds != null && subjectIds.Any())
            {
                foreach (var subjectId in subjectIds)
                {
                    await _context.TeacherSubjects.AddAsync(new TeacherSubject
                    {
                        TeacherProfileId = teacherProfile.Id,
                        SubjectId = subjectId
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Добавляем программы подготовки репетитора
            if (preparationProgramIds != null && preparationProgramIds.Any())
            {
                foreach (var programId in preparationProgramIds)
                {
                    // Получаем шаблонную программу подготовки
                    var templateProgram = await _context.PreparationPrograms
                        .FirstOrDefaultAsync(p => p.Id == programId && p.TeacherProfileId == null);
                    
                    if (templateProgram != null)
                    {
                        // Создаем копию программы для конкретного учителя
                        await _context.PreparationPrograms.AddAsync(new PreparationProgram
                        {
                            TeacherProfileId = teacherProfile.Id,
                            Name = templateProgram.Name,
                            Description = templateProgram.Description
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return (true, "Регистрация репетитора успешно завершена.", user, teacherProfile);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка при регистрации: {ex.Message}", null, null);
        }
    }

    public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string login, string password)
    {
        var user = await _userManager.FindByNameAsync(login);
        if (user == null)
            return (false, "Пользователь не найден", null, null);

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
            return (false, "Неверный пароль", null, null);

        var token = await GenerateJwtTokenAsync(user);
        return (true, "Авторизация успешна", token, user);
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("Role", user.Role)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var keyString = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            throw new InvalidOperationException("JWT Key is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 