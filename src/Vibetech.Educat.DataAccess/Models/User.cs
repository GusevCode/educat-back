using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Vibetech.Educat.DataAccess.Models;

public class User : IdentityUser<int>
{
    [Required]
    [StringLength(512)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(512)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(512)]
    public string? MiddleName { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty; // "Male" или "Female"

    [MaxLength(300 * 1024)] // 300 KB
    public string? PhotoBase64 { get; set; }

    [StringLength(1000)]
    public string? ContactInformation { get; set; }

    [Required]
    public string Role { get; set; } = "Student"; // "Student" или "Teacher"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    [Required]
    public string Login { get; set; } = string.Empty;

    // Навигационные свойства
    public virtual ICollection<TeacherProfile> TeacherProfiles { get; set; } = new List<TeacherProfile>();
    public virtual ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();
    public virtual ICollection<TeacherStudent> Students { get; set; } = new List<TeacherStudent>();
    public virtual ICollection<Lesson> TeacherLessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Lesson> StudentLessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}