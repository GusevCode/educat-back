using Microsoft.AspNetCore.Mvc;
using Vibetech.Educat.DataAccess;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.API.Models;
using Vibetech.Educat.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibetech.Educat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Справочники системы")]
public class DictionaryController : ControllerBase
{
    private readonly EducatDbContext _context;

    public DictionaryController(EducatDbContext context)
    {
        _context = context;
    }

    [HttpGet("subjects")]
    [SwaggerOperation(Summary = "Получение списка предметов", Description = "Возвращает список всех доступных предметов")]
    [SwaggerResponse(200, "Список предметов", typeof(IEnumerable<SubjectDto>))]
    public async Task<IActionResult> GetSubjects()
    {
        var subjects = await _context.Subjects
            .Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

        return Ok(subjects);
    }

    [HttpGet("preparation-programs")]
    [SwaggerOperation(Summary = "Получение списка программ подготовки", Description = "Возвращает список всех доступных программ подготовки")]
    [SwaggerResponse(200, "Список программ подготовки", typeof(IEnumerable<PreparationProgramDto>))]
    public async Task<IActionResult> GetPreparationPrograms()
    {
        var programs = await _context.PreparationPrograms
            .Where(p => p.TeacherProfileId == null)
            .Select(p => new PreparationProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty
            })
            .ToListAsync();

        return Ok(programs);
    }

    [HttpPost("subjects")]
    [SwaggerOperation(Summary = "Добавление предмета", Description = "Добавляет новый предмет в систему")]
    [SwaggerResponse(200, "Предмет успешно добавлен", typeof(SubjectDto))]
    public async Task<IActionResult> AddSubject([FromBody] AddSubjectRequest request)
    {
        var subject = new Subject
        {
            Name = request.Name
        };

        await _context.Subjects.AddAsync(subject);
        await _context.SaveChangesAsync();

        return Ok(new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name
        });
    }

    [HttpPost("preparation-programs")]
    [SwaggerOperation(Summary = "Добавление программы подготовки", Description = "Добавляет новую программу подготовки в систему")]
    [SwaggerResponse(200, "Программа подготовки успешно добавлена", typeof(PreparationProgramDto))]
    public async Task<IActionResult> AddPreparationProgram([FromBody] AddPreparationProgramRequest request)
    {
        // Проверяем, существует ли уже такая программа
        var existingProgram = await _context.PreparationPrograms
            .FirstOrDefaultAsync(p => p.Name == request.Name && p.TeacherProfileId == null);
            
        if (existingProgram != null)
        {
            return BadRequest(new { Message = "Программа подготовки с таким названием уже существует" });
        }
            
        var program = new PreparationProgram
        {
            Name = request.Name,
            Description = request.Description,
            TeacherProfileId = null // Создаем шаблонную программу
        };

        await _context.PreparationPrograms.AddAsync(program);
        await _context.SaveChangesAsync();

        return Ok(new PreparationProgramDto
        {
            Id = program.Id,
            Name = program.Name,
            Description = program.Description ?? string.Empty
        });
    }
}

public class AddSubjectRequest
{
    public string Name { get; set; } = string.Empty;
}

public class AddPreparationProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 