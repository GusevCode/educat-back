using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Attachment;

public class AttachmentRepository : BaseRepository<Models.Attachment>, IAttachmentRepository
{
    public AttachmentRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.Attachment>> GetByLessonIdAsync(int lessonId)
    {
        return await _dbSet
            .Include(a => a.Lesson)
            .Where(a => a.LessonId == lessonId)
            .ToListAsync();
    }

    public async Task<bool> DeleteAllByLessonIdAsync(int lessonId)
    {
        var attachments = await _dbSet
            .Where(a => a.LessonId == lessonId)
            .ToListAsync();
            
        if (!attachments.Any()) return false;
        
        _dbSet.RemoveRange(attachments);
        await _context.SaveChangesAsync();
        return true;
    }
}