using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Lesson;

public interface ILessonRepository : IRepository<Models.Lesson>
{
    Task<IEnumerable<Models.Lesson>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<Models.Lesson>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<Models.Lesson>> GetBySubjectIdAsync(int subjectId);
    Task<IEnumerable<Models.Lesson>> GetByStatusAsync(LessonStatus status);
    Task<IEnumerable<Models.Lesson>> GetUpcomingLessonsForTeacherAsync(int teacherId);
    Task<IEnumerable<Models.Lesson>> GetUpcomingLessonsForStudentAsync(int studentId);
    Task<IEnumerable<Models.Lesson>> GetPastLessonsForTeacherAsync(int teacherId);
    Task<IEnumerable<Models.Lesson>> GetPastLessonsForStudentAsync(int studentId);
    Task<Models.Lesson?> UpdateLessonStatusAsync(int lessonId, LessonStatus status);
} 