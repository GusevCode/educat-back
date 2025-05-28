using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.DataAccess.Models;

public class TeacherProfile : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(512)]
    public string Education { get; set; } = string.Empty;

    [Required]
    [Range(0, 50)]
    public int ExperienceYears { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }

    public double Rating { get; set; } = 0;
    public int ReviewsCount { get; set; } = 0;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public virtual ICollection<TeacherStudent> Students { get; set; } = new List<TeacherStudent>();
    public virtual ICollection<Lesson> TeacherLessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<PreparationProgram> PreparationPrograms { get; set; } = new List<PreparationProgram>();
} 