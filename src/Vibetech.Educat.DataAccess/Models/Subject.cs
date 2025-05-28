using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class Subject : BaseEntity
{
    [Required]
    [StringLength(512)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
} 