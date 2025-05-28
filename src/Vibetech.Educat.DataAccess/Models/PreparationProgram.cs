namespace Vibetech.Educat.DataAccess.Models;

public class PreparationProgram : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    
    public int? TeacherProfileId { get; set; }
    public virtual TeacherProfile? TeacherProfile { get; set; }
}