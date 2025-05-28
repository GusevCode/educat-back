using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherStudent;

public class TeacherStudentRepository : BaseRepository<Models.TeacherStudent>, ITeacherStudentRepository
{
    public TeacherStudentRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.TeacherStudent>> GetByTeacherIdAsync(int teacherId)
    {
        return await _dbSet
            .Include(ts => ts.Student)
            .Where(ts => ts.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TeacherStudent>> GetByStudentIdAsync(int studentId)
    {
        return await _dbSet
            .Include(ts => ts.Teacher)
            .Where(ts => ts.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<Models.TeacherStudent?> GetByTeacherAndStudentIdAsync(int teacherId, int studentId)
    {
        return await _dbSet
            .Include(ts => ts.Student)
            .Include(ts => ts.Teacher)
            .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
    }

    public async Task<bool> DeleteByTeacherAndStudentIdAsync(int teacherId, int studentId)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(ts => 
            ts.TeacherId == teacherId && ts.StudentId == studentId);
            
        if (entity == null) return false;
        
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
} 