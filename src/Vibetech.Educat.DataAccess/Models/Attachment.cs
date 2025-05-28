using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class Attachment : BaseEntity
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FileType { get; set; } = string.Empty;
    
    [Required]
    public string Base64Content { get; set; } = string.Empty;
    
    [Required]
    public long Size { get; set; }
    
    [Required]
    public int LessonId { get; set; }
    
    // Навигационные свойства
    public virtual Lesson Lesson { get; set; } = null!;
} 