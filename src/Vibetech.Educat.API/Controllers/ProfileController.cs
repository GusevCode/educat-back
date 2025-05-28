using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vibetech.Educat.Services.Services;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.API.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Vibetech.Educat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("API для доступа к профилям")]
public class ProfileController : ControllerBase
{
    private readonly TeacherService _teacherService;
    private readonly StudentService _studentService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        TeacherService teacherService, 
        StudentService studentService,
        IMapper mapper, 
        ILogger<ProfileController> logger)
    {
        _teacherService = teacherService;
        _studentService = studentService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("teacher/{teacherId}")]
    [SwaggerOperation(Summary = "Получение профиля преподавателя", Description = "Возвращает данные профиля преподавателя по ID")]
    [SwaggerResponse(200, "Профиль преподавателя", typeof(TeacherProfileDto))]
    [SwaggerResponse(404, "Преподаватель не найден")]
    public async Task<IActionResult> GetTeacherProfile(int teacherId)
    {
        _logger.LogInformation("Получен запрос профиля преподавателя с ID: {TeacherId}", teacherId);
        
        var profile = await _teacherService.GetTeacherProfileAsync(teacherId);
        
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

    [HttpGet("student/{studentId}")]
    [SwaggerOperation(Summary = "Получение профиля студента", Description = "Возвращает данные профиля студента по ID")]
    [SwaggerResponse(200, "Профиль студента", typeof(StudentProfileDto))]
    [SwaggerResponse(404, "Студент не найден")]
    public async Task<IActionResult> GetStudentProfile(int studentId)
    {
        _logger.LogInformation("Получен запрос профиля студента с ID: {StudentId}", studentId);
        
        var profile = await _studentService.GetStudentProfileAsync(studentId);
        
        if (profile == null)
        {
            _logger.LogWarning("Профиль студента с ID: {StudentId} не найден", studentId);
            return NotFound();
        }
            
        var mappedProfile = _mapper.Map<StudentProfileDto>(profile);
        
        // Добавляем роли в UserDto, если он существует
        if (mappedProfile.User != null)
        {
            mappedProfile.User.IsTeacher = false;
            mappedProfile.User.Roles = new List<string> { "STUDENT" };
        }
        
        return Ok(mappedProfile);
    }
} 