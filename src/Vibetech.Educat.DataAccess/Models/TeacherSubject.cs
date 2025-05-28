using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class TeacherSubject : BaseEntity
{
    [Required]
    public int TeacherProfileId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
} 