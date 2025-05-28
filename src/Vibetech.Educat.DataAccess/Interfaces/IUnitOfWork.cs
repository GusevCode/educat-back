using Vibetech.Educat.DataAccess.Repositories.Attachment;
using Vibetech.Educat.DataAccess.Repositories.Lesson;
using Vibetech.Educat.DataAccess.Repositories.PreparationProgram;
using Vibetech.Educat.DataAccess.Repositories.Review;
using Vibetech.Educat.DataAccess.Repositories.Subject;
using Vibetech.Educat.DataAccess.Repositories.TeacherProfile;
using Vibetech.Educat.DataAccess.Repositories.TeacherStudent;
using Vibetech.Educat.DataAccess.Repositories.TeacherSubject;

namespace Vibetech.Educat.DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ISubjectRepository Subjects { get; }
    ITeacherProfileRepository TeacherProfiles { get; }
    ITeacherSubjectRepository TeacherSubjects { get; }
    ITeacherStudentRepository TeacherStudents { get; }
    ILessonRepository Lessons { get; }
    IAttachmentRepository Attachments { get; }
    IReviewRepository Reviews { get; }
    IPreparationProgramRepository PreparationPrograms { get; }

    Task<int> SaveChangesAsync();
} 