using Vibetech.Educat.DataAccess.Interfaces;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Review;

public interface IReviewRepository : IRepository<Models.Review>
{
    Task<IEnumerable<Models.Review>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<Models.Review>> GetByStudentIdAsync(int studentId);
    Task<double> GetAverageRatingByTeacherIdAsync(int teacherId);
} 