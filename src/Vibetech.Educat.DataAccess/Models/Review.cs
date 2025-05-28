using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class Review : BaseEntity
{
    [Required]
    public int LessonId { get; set; }
    
    [Required]
    public int TeacherId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;
    
    // Навигационные свойства
    public virtual User Teacher { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
} 