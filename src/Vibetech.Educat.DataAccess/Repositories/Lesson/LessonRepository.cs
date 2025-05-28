using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Lesson;

public class LessonRepository : BaseRepository<Models.Lesson>, ILessonRepository
{
    public LessonRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.Lesson>> GetByTeacherIdAsync(int teacherId)
    {
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Include(l => l.Attachments)
            .Where(l => l.TeacherId == teacherId)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetByStudentIdAsync(int studentId)
    {
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Include(l => l.Attachments)
            .Where(l => l.StudentId == studentId)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetBySubjectIdAsync(int subjectId)
    {
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.SubjectId == subjectId)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetByStatusAsync(LessonStatus status)
    {
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.Status == status)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetUpcomingLessonsForTeacherAsync(int teacherId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.TeacherId == teacherId && l.StartTime > now)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetUpcomingLessonsForStudentAsync(int studentId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.StudentId == studentId && l.StartTime > now)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetPastLessonsForTeacherAsync(int teacherId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.TeacherId == teacherId && l.EndTime < now)
            .OrderByDescending(l => l.EndTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Lesson>> GetPastLessonsForStudentAsync(int studentId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(l => l.Teacher)
            .Include(l => l.Student)
            .Include(l => l.Subject)
            .Where(l => l.StudentId == studentId && l.EndTime < now)
            .OrderByDescending(l => l.EndTime)
            .ToListAsync();
    }

    public async Task<Models.Lesson?> UpdateLessonStatusAsync(int lessonId, LessonStatus status)
    {
        var lesson = await _dbSet.FindAsync(lessonId);
        if (lesson == null) return null;

        lesson.Status = status;
        lesson.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return lesson;
    }
} 