using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.PreparationProgram;

public interface IPreparationProgramRepository : IRepository<Models.PreparationProgram>
{
    Task<IEnumerable<Models.PreparationProgram>> GetByTeacherProfileIdAsync(int teacherProfileId);
    Task<bool> DeleteByTeacherProfileIdAsync(int teacherProfileId);
} 