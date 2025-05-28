using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Attachment;

public interface IAttachmentRepository : IRepository<Models.Attachment>
{
    Task<IEnumerable<Models.Attachment>> GetByLessonIdAsync(int lessonId);
    Task<bool> DeleteAllByLessonIdAsync(int lessonId);
} 