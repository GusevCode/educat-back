using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.API.Models;

public class LessonDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    
    [SwaggerSchema(Format = "date-time", Description = "Время начала урока в формате ISO 8601 (например, 2023-05-25T14:30:00Z)")]
    public DateTime StartTime { get; set; }
    
    [SwaggerSchema(Format = "date-time", Description = "Время окончания урока в формате ISO 8601 (например, 2023-05-25T16:00:00Z)")]
    public DateTime EndTime { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public string ConferenceLink { get; set; } = string.Empty;
    public string WhiteboardLink { get; set; } = string.Empty;
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int Size { get; set; }
    
    // Содержимое файла в формате Base64
    public string Base64Content { get; set; } = string.Empty;
}

[SwaggerSchema(Description = "Запрос на создание урока. Поддерживается создание пересекающихся уроков")]
public class CreateLessonRequest
{
    [Required]
    public int TeacherId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
    
    [Required]
    [SwaggerSchema(Format = "date-time", Description = "Время начала урока в формате ISO 8601 (например, 2023-05-25T14:30:00Z)")]
    public DateTime StartTime { get; set; }
    
    [Required]
    [SwaggerSchema(Format = "date-time", Description = "Время окончания урока в формате ISO 8601 (например, 2023-05-25T16:00:00Z)")]
    public DateTime EndTime { get; set; }
    
    public string ConferenceLink { get; set; } = string.Empty;
    
    public string WhiteboardLink { get; set; } = string.Empty;
}

public class UpdateLessonRequest
{
    [SwaggerSchema(Format = "date-time", Description = "Время начала урока")]
    public DateTime? StartTime { get; set; }
    
    [SwaggerSchema(Format = "date-time", Description = "Время окончания урока")]
    public DateTime? EndTime { get; set; }
    
    public LessonStatus? Status { get; set; }
    public string ConferenceLink { get; set; } = string.Empty;
    public string WhiteboardLink { get; set; } = string.Empty;
}

public class UploadAttachmentRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FileType { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Содержимое файла в формате Base64 (временно, будет заменено на загрузку через multipart/form-data)")]
    public string Base64Content { get; set; } = string.Empty;
}

public class LessonFilterRequest
{
    public int? TeacherId { get; set; }
    public int? StudentId { get; set; }
    public int? SubjectId { get; set; }
    
    [SwaggerSchema(Format = "date", Description = "Начальная дата фильтра в формате YYYY-MM-DD")]
    public DateTime? StartDate { get; set; }
    
    [SwaggerSchema(Format = "date", Description = "Конечная дата фильтра в формате YYYY-MM-DD")]
    public DateTime? EndDate { get; set; }
    
    public LessonStatus? Status { get; set; }
    public bool SortNewestFirst { get; set; } = true;
} 