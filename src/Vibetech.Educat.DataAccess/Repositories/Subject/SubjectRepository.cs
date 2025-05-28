using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Subject;

public class SubjectRepository : BaseRepository<Models.Subject>, ISubjectRepository
{
    public SubjectRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.Subject>> GetSubjectsByTeacherIdAsync(int teacherId)
    {
        return await _context.TeacherSubjects
            .Where(ts => ts.TeacherProfileId == teacherId)
            .Include(ts => ts.Subject)
            .Select(ts => ts.Subject)
            .ToListAsync();
    }
} 