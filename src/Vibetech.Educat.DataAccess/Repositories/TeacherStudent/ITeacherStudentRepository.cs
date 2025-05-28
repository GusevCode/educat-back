using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.TeacherStudent;

public interface ITeacherStudentRepository : IRepository<Models.TeacherStudent>
{
    Task<IEnumerable<Models.TeacherStudent>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<Models.TeacherStudent>> GetByStudentIdAsync(int studentId);
    Task<Models.TeacherStudent?> GetByTeacherAndStudentIdAsync(int teacherId, int studentId);
    Task<bool> DeleteByTeacherAndStudentIdAsync(int teacherId, int studentId);
} 