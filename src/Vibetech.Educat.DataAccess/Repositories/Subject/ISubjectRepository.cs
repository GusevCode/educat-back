using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Subject;

public interface ISubjectRepository : IRepository<Models.Subject>
{
    Task<IEnumerable<Models.Subject>> GetSubjectsByTeacherIdAsync(int teacherId);
} 