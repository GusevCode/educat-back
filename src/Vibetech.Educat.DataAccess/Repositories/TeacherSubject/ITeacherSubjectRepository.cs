using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherSubject;

public interface ITeacherSubjectRepository : IRepository<Models.TeacherSubject>
{
    Task<IEnumerable<Models.TeacherSubject>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<Models.TeacherSubject>> GetBySubjectIdAsync(int subjectId);
    Task<Models.TeacherSubject?> GetByTeacherAndSubjectIdAsync(int teacherId, int subjectId);
    Task<bool> DeleteByTeacherAndSubjectIdAsync(int teacherId, int subjectId);
} 