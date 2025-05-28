using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class TeacherStudent : BaseEntity
{
    [Required]
    public int TeacherId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? AcceptedDate { get; set; }
    
    [Required]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    
    // Навигационные свойства
    public virtual User Teacher { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
} 