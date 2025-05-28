using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.PreparationProgram;

public class PreparationProgramRepository : BaseRepository<Models.PreparationProgram>, IPreparationProgramRepository
{
    public PreparationProgramRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.PreparationProgram>> GetByTeacherProfileIdAsync(int teacherProfileId)
    {
        return await _dbSet
            .Include(pp => pp.TeacherProfile)
            .Where(pp => pp.TeacherProfileId == teacherProfileId)
            .ToListAsync();
    }

    public async Task<bool> DeleteByTeacherProfileIdAsync(int teacherProfileId)
    {
        var programs = await _dbSet
            .Where(pp => pp.TeacherProfileId == teacherProfileId)
            .ToListAsync();
            
        if (!programs.Any()) return false;
        
        _dbSet.RemoveRange(programs);
        await _context.SaveChangesAsync();
        return true;
    }
} 