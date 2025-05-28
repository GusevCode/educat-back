using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherProfile;

public class TeacherProfileRepository : BaseRepository<Models.TeacherProfile>, ITeacherProfileRepository
{
    public TeacherProfileRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<Models.TeacherProfile?> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(tp => tp.User)
            .Include(tp => tp.TeacherSubjects)
            .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(tp => tp.UserId == userId);
    }

    public async Task<IEnumerable<Models.TeacherProfile>> GetBySubjectIdAsync(int subjectId)
    {
        return await _context.TeacherSubjects
            .Where(ts => ts.SubjectId == subjectId)
            .Include(ts => ts.TeacherProfile)
            .ThenInclude(tp => tp.User)
            .Select(ts => ts.TeacherProfile)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TeacherProfile>> GetModeratedTeacherProfilesAsync()
    {
        return await _dbSet
            .Include(tp => tp.User)
            .Include(tp => tp.TeacherSubjects)
            .ThenInclude(ts => ts.Subject)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TeacherProfile>> GetNonModeratedTeacherProfilesAsync()
    {
        return await _dbSet
            .Include(tp => tp.User)
            .Include(tp => tp.TeacherSubjects)
            .ThenInclude(ts => ts.Subject)
            .ToListAsync();
    }

    public async Task<Models.TeacherProfile?> UpdateTeacherProfileWithRatingAsync(int teacherProfileId, double rating)
    {
        var teacherProfile = await _dbSet.FindAsync(teacherProfileId);
        if (teacherProfile == null) return null;

        teacherProfile.Rating = rating;
        teacherProfile.ReviewsCount += 1;
        teacherProfile.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return teacherProfile;
    }
} 