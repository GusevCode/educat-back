using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherSubject;

public class TeacherSubjectRepository : BaseRepository<Models.TeacherSubject>, ITeacherSubjectRepository
{
    public TeacherSubjectRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.TeacherSubject>> GetByTeacherIdAsync(int teacherId)
    {
        return await _dbSet
            .Include(ts => ts.Subject)
            .Where(ts => ts.TeacherProfileId == teacherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TeacherSubject>> GetBySubjectIdAsync(int subjectId)
    {
        return await _dbSet
            .Include(ts => ts.TeacherProfile)
            .ThenInclude(tp => tp.User)
            .Where(ts => ts.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<Models.TeacherSubject?> GetByTeacherAndSubjectIdAsync(int teacherId, int subjectId)
    {
        return await _dbSet
            .Include(ts => ts.Subject)
            .Include(ts => ts.TeacherProfile)
            .FirstOrDefaultAsync(ts => ts.TeacherProfileId == teacherId && ts.SubjectId == subjectId);
    }

    public async Task<bool> DeleteByTeacherAndSubjectIdAsync(int teacherId, int subjectId)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(ts => 
            ts.TeacherProfileId == teacherId && ts.SubjectId == subjectId);
            
        if (entity == null) return false;
        
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
} 