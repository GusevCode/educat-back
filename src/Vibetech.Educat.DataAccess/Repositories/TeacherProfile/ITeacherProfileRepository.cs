using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherProfile;

public interface ITeacherProfileRepository : IRepository<Models.TeacherProfile>
{
    Task<Models.TeacherProfile?> GetByUserIdAsync(int userId);
    Task<IEnumerable<Models.TeacherProfile>> GetBySubjectIdAsync(int subjectId);
    Task<IEnumerable<Models.TeacherProfile>> GetModeratedTeacherProfilesAsync();
    Task<IEnumerable<Models.TeacherProfile>> GetNonModeratedTeacherProfilesAsync();
    Task<Models.TeacherProfile?> UpdateTeacherProfileWithRatingAsync(int teacherProfileId, double rating);
} 