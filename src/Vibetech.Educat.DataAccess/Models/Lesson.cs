using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vibetech.Educat.DataAccess.Models;

public class Lesson : BaseEntity
{
    /// <summary>
    /// ID учителя (User.Id, а не TeacherProfile.Id)
    /// </summary>
    [Required]
    public int TeacherId { get; set; }
    
    [Required]
    public int? StudentId { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    public LessonStatus Status { get; set; }
    
    public string? ConferenceLink { get; set; }
    
    public string? WhiteboardLink { get; set; }
    
    /// <summary>
    /// Актуальный статус урока с учетом текущего времени
    /// </summary>
    [NotMapped]
    public LessonStatus ActualStatus
    {
        get
        {
            var currentTime = DateTime.UtcNow;
            
            // Если статус уже Completed или Cancelled, оставляем его
            if (Status == LessonStatus.Completed || Status == LessonStatus.Cancelled)
                return Status;
            
            // Если время окончания урока уже прошло, возвращаем Completed
            if (EndTime < currentTime && Status == LessonStatus.Scheduled)
                return LessonStatus.Completed;
            
            // Если текущее время между началом и окончанием урока, возвращаем InProgress
            if (StartTime <= currentTime && EndTime > currentTime && Status == LessonStatus.Scheduled)
                return LessonStatus.InProgress;
            
            return Status;
        }
    }
    
    /// <summary>
    /// Актуальный статус урока в виде строки
    /// </summary>
    [NotMapped]
    public string ActualStatusString => ActualStatus.ToString();
    
    // Навигационные свойства
    public virtual User Teacher { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
} 